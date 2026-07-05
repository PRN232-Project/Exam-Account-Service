using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PRN232.ExamAccount.Application.Interfaces;
using PRN232.ExamAccount.Application.Messaging;
using PRN232.ExamAccount.Domain.Enums;
using PRN232.ExamAccount.Infrastructure.Persistence;
using RabbitMQ.Client;

namespace PRN232.ExamAccount.Infrastructure.Messaging;

public class RabbitMqJobPublisher : IGradingJobPublisher
{
    private readonly ExamAccountDbContext _dbContext;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqJobPublisher> _logger;

    public RabbitMqJobPublisher(
        ExamAccountDbContext dbContext,
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqJobPublisher> logger)
    {
        _dbContext = dbContext;
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishSubmissionAsync(Guid submissionId, bool isRegrade, CancellationToken cancellationToken = default)
    {
        var submission = await _dbContext.Submissions
            .Include(x => x.Exam)
                .ThenInclude(x => x!.Sections)
            .Include(x => x.StudentAccount)
            .FirstOrDefaultAsync(x => x.Id == submissionId, cancellationToken);

        if (submission is null || submission.Exam is null || submission.StudentAccount is null)
        {
            throw new InvalidOperationException($"Submission {submissionId} không tồn tại hoặc thiếu dữ liệu liên kết.");
        }

        var message = new GradingJobMessage
        {
            SubmissionId = submission.Id,
            ExamId = submission.ExamId,
            StudentId = submission.StudentAccountId,
            StudentCode = submission.StudentAccount.StudentCode,
            WorkspacePath = submission.WorkspacePath,
            IsRegrade = isRegrade,
            RequestedAtUtc = DateTime.UtcNow,
            ExamConfig = new ExamConfigurationMessage
            {
                ExamId = submission.Exam.Id,
                ExamCode = submission.Exam.Code,
                ExamTitle = submission.Exam.Title,
                MaxScore = submission.Exam.MaxScore,
                SolutionPattern = submission.Exam.SolutionPattern,
                RequireAppSettings = submission.Exam.RequireAppSettings,
                ForbidHardcodedConnectionString = submission.Exam.ForbidHardcodedConnectionString,
                TimeoutSeconds = submission.Exam.TimeoutSeconds,
                PlagiarismConfig = submission.Exam.PlagiarismKeywords,
                Sections = submission.Exam.Sections
                    .OrderBy(x => x.Name)
                    .Select(x => new SectionConfigurationMessage
                    {
                        Name = x.Name,
                        Weight = x.Weight,
                        TestFilter = x.TestFilter
                    })
                    .ToList()
            }
        };

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        DeclareTopology(channel);

        var routingKey = isRegrade ? _options.RegradeRoutingKey : _options.NewJobRoutingKey;
        submission.Status = isRegrade ? SubmissionStatus.RegradingRequested : SubmissionStatus.QueuedForGrading;

        var payload = JsonSerializer.SerializeToUtf8Bytes(message);
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        channel.BasicPublish(_options.ExchangeName, routingKey, properties, payload);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Published grading job for submission {SubmissionId} with routing key {RoutingKey}.",
            submissionId,
            routingKey);
    }

    private void DeclareTopology(IModel channel)
    {
        channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Direct, durable: true);
        channel.QueueDeclare(_options.GradingJobsQueue, durable: true, exclusive: false, autoDelete: false);
        channel.QueueDeclare(_options.RegradeQueue, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(_options.GradingJobsQueue, _options.ExchangeName, _options.NewJobRoutingKey);
        channel.QueueBind(_options.RegradeQueue, _options.ExchangeName, _options.RegradeRoutingKey);
    }
}
