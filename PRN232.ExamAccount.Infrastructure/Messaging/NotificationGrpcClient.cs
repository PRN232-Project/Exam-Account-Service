using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PRN232.Notification.Grpc;

namespace PRN232.ExamAccount.Infrastructure.Messaging;

public class NotificationGrpcClient
{
    private readonly NotificationGrpcOptions _options;
    private readonly ILogger<NotificationGrpcClient> _logger;

    public NotificationGrpcClient(
        IOptions<NotificationGrpcOptions> options,
        ILogger<NotificationGrpcClient> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendNotificationAsync(
        Guid lecturerId,
        Guid roomId,
        Guid examId,
        Guid submissionId,
        string type,
        string title,
        string message,
        CancellationToken cancellationToken = default)
    {
        using var channel = GrpcChannel.ForAddress(_options.Address);
        var client = new NotificationGrpcService.NotificationGrpcServiceClient(channel);

        var response = await client.SendNotificationAsync(
            new SendNotificationRequest
            {
                LecturerId = lecturerId.ToString(),
                RoomId = roomId.ToString(),
                ExamId = examId.ToString(),
                SubmissionId = submissionId.ToString(),
                Type = type,
                Title = title,
                Message = message
            },
            cancellationToken: cancellationToken);

        _logger.LogInformation("Notification gRPC sent for submission {SubmissionId}: {Result}", submissionId, response.Success);
    }
}
