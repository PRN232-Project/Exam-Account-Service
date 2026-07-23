using System.Text;
using System.Text.Json;
using PRN232.ExamAccount.Domain.Entities;
using RabbitMQ.Client;

namespace PRN232.ExamAccount.Api.Integration;

public sealed class RealtimeNotificationClient(IConfiguration configuration, ILogger<RealtimeNotificationClient> logger)
{
    public async Task SendAsync(NotificationRecord notification, Guid? examSessionId, Guid? roomId, CancellationToken ct)
    {
        if (!configuration.GetValue("RabbitMQ:Enabled", true)) return;
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
                Port = configuration.GetValue("RabbitMQ:Port", 5672),
                UserName = configuration["RabbitMQ:UserName"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest"
            };
            await using var connection = await factory.CreateConnectionAsync(ct);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);
            var queue = notification.Type == "BatchAssigned" ? "grading-jobs" : "grading-results";
            await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
            var payload = JsonSerializer.Serialize(new
            {
                notification.Id,
                notification.RecipientUserId,
                notification.GradingBatchId,
                notification.GradingItemId,
                ExamId = examSessionId,
                RoomId = roomId,
                notification.Type,
                notification.Title,
                notification.Message,
                notification.CreatedAtUtc
            });
            await channel.BasicPublishAsync(string.Empty, queue, true, Encoding.UTF8.GetBytes(payload), ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "RabbitMQ unavailable; persistent notification remains in Central.");
        }
    }
}
