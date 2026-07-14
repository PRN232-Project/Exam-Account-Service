using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PRN232.ExamAccount.Application.Messaging;
using PRN232.ExamAccount.Domain.Entities;
using PRN232.ExamAccount.Domain.Enums;
using PRN232.ExamAccount.Infrastructure.Persistence;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PRN232.ExamAccount.Infrastructure.Messaging;

public class GradingResultConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly NotificationGrpcClient _notificationGrpcClient;
    private readonly ILogger<GradingResultConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public GradingResultConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        NotificationGrpcClient notificationGrpcClient,
        ILogger<GradingResultConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _notificationGrpcClient = notificationGrpcClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _connection ??= factory.CreateConnection();
                _channel ??= _connection.CreateModel();

                _channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Direct, durable: true);
                _channel.QueueDeclare(_options.GradingResultsQueue, durable: true, exclusive: false, autoDelete: false);
                _channel.QueueBind(_options.GradingResultsQueue, _options.ExchangeName, _options.ResultRoutingKey);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += OnMessageReceivedAsync;

                _channel.BasicConsume(_options.GradingResultsQueue, autoAck: false, consumer);
                _logger.LogInformation("GradingResultConsumer connected to RabbitMQ and started consuming.");
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cannot connect to RabbitMQ yet. Retrying in 5 seconds.");
                _channel?.Dispose();
                _connection?.Dispose();
                _channel = null;
                _connection = null;
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
    {
        if (_channel is null)
        {
            return;
        }

        try
        {
            var json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            _logger.LogInformation("Received grading result payload: {Payload}", json);

            var message = JsonSerializer.Deserialize<SubmissionGradedEvent>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (message is null)
            {
                throw new InvalidOperationException("Không deserialize được SubmissionGradedEvent.");
            }

            await ProcessMessageAsync(message, CancellationToken.None);
            _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
            _logger.LogInformation("Processed grading result for submission {SubmissionId}.", message.SubmissionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process grading result message.");
            _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true);
        }
    }

    protected internal Task InvokeProcessAsync(SubmissionGradedEvent message, CancellationToken cancellationToken = default)
    {
        return ProcessMessageAsync(message, cancellationToken);
    }

    private async Task ProcessMessageAsync(SubmissionGradedEvent message, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ExamAccountDbContext>();
        _logger.LogInformation("Loading submission {SubmissionId} for grading result sync.", message.SubmissionId);

        var submission = await dbContext.Submissions
            .Include(x => x.Exam)
                .ThenInclude(x => x!.Room)
            .FirstOrDefaultAsync(x => x.Id == message.SubmissionId, cancellationToken);

        if (submission is null)
        {
            throw new InvalidOperationException($"Không tìm thấy submission {message.SubmissionId} để cập nhật kết quả.");
        }

        _logger.LogInformation("Loaded submission {SubmissionId} with current status {Status}.", submission.Id, submission.Status);

        submission.TotalScore = message.TotalScore;
        submission.RawJsonReport = message.RawJsonReport;
        submission.ErrorMessage = message.ErrorMessage;
        submission.GradedAtUtc = message.CompletedAtUtc;
        submission.Status = message.HasErrors
            ? SubmissionStatus.Failed
            : SubmissionStatus.GradedPendingPublication;

        var existingSectionResults = await dbContext.SubmissionSectionResults
            .Where(x => x.SubmissionId == message.SubmissionId)
            .ToListAsync(cancellationToken);

        if (existingSectionResults.Count > 0)
        {
            dbContext.SubmissionSectionResults.RemoveRange(existingSectionResults);
        }

        var parsedSectionResults = ParseSectionResults(message.SubmissionId, message.RawJsonReport).ToList();
        _logger.LogInformation(
            "Replacing {ExistingCount} section results with {NewCount} parsed items for submission {SubmissionId}.",
            existingSectionResults.Count,
            parsedSectionResults.Count,
            message.SubmissionId);

        if (parsedSectionResults.Count > 0)
        {
            await dbContext.SubmissionSectionResults.AddRangeAsync(parsedSectionResults, cancellationToken);
        }

        if (message.HasErrors && submission.Exam?.Room is not null)
        {
            var lecturerId = submission.Exam.Room.LecturerId;
            var notification = new NotificationRecord
            {
                Id = Guid.NewGuid(),
                RecipientUserId = lecturerId,
                SubmissionId = submission.Id,
                ExamId = submission.ExamId,
                RoomId = submission.Exam.RoomId,
                Type = "GradingFailed",
                Title = $"Submission {message.StudentCode} grading failed",
                Message = string.IsNullOrWhiteSpace(message.ErrorMessage)
                    ? "Bai thi gap loi trong qua trinh cham."
                    : message.ErrorMessage,
                IsRead = false,
                CreatedAtUtc = DateTime.UtcNow
            };

            await dbContext.Notifications.AddAsync(notification, cancellationToken);

            await _notificationGrpcClient.SendNotificationAsync(
                lecturerId,
                submission.Exam.RoomId,
                submission.ExamId,
                submission.Id,
                notification.Type,
                notification.Title,
                notification.Message,
                cancellationToken);
        }

        _logger.LogInformation("Saving grading result changes for submission {SubmissionId}.", message.SubmissionId);
        await dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Saved grading result changes for submission {SubmissionId}.", message.SubmissionId);
    }

    private static IEnumerable<SubmissionSectionResult> ParseSectionResults(Guid submissionId, string rawJsonReport)
    {
        if (string.IsNullOrWhiteSpace(rawJsonReport))
        {
            return [];
        }

        using var document = JsonDocument.Parse(rawJsonReport);
        if (!document.RootElement.TryGetProperty("sectionResults", out var sectionResultsElement) ||
            sectionResultsElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var results = new List<SubmissionSectionResult>();

        foreach (var element in sectionResultsElement.EnumerateArray())
        {
            results.Add(new SubmissionSectionResult
            {
                Id = Guid.NewGuid(),
                SubmissionId = submissionId,
                SectionName = GetString(element, "name"),
                Score = GetDecimal(element, "score"),
                MaxScore = GetDecimal(element, "maxScore"),
                Status = GetString(element, "status"),
                Feedback = GetString(element, "feedback")
            });
        }

        return results;
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }

    private static decimal GetDecimal(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return 0m;
        }

        return property.ValueKind switch
        {
            JsonValueKind.Number when property.TryGetDecimal(out var value) => value,
            JsonValueKind.String when decimal.TryParse(property.GetString(), out var value) => value,
            _ => 0m
        };
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
