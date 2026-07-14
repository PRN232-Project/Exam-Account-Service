using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PRN232.ExamAccount.Application.Messaging;
using PRN232.ExamAccount.Domain.Entities;
using PRN232.ExamAccount.Domain.Enums;
using PRN232.ExamAccount.Infrastructure.Messaging;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Tests;

public class GradingResultConsumerTests
{
    private const string ExamCode = "PRN232";
    private const string ExamTitle = "Exam";
    private const string SolutionPattern = "*.sln";
    private const string StudentCode = "SE0001";
    private const string StudentName = "Student 1";
    private const string WorkspacePath = "workspace";
    private const string SectionName = "CRUD";
    private const string SectionStatus = "Passed";
    private const string SectionFeedback = "OK";
    private const decimal SectionScore = 3.5m;
    private const decimal SectionMaxScore = 4m;
    private const decimal TotalScore = 8.5m;

    [Fact]
    public async Task ProcessForTestAsync_UpdatesSubmissionAndSectionResults()
    {
        var databaseName = Guid.NewGuid().ToString();
        var databaseRoot = new InMemoryDatabaseRoot();
        var examId = Guid.NewGuid();
        var lecturerId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var submissionId = Guid.NewGuid();

        var services = new ServiceCollection();
        services.AddDbContext<ExamAccountDbContext>(options => options.UseInMemoryDatabase(databaseName, databaseRoot));
        var provider = services.BuildServiceProvider();

        await using (var scope = provider.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ExamAccountDbContext>();

            dbContext.Students.Add(new StudentAccount
            {
                Id = lecturerId,
                UserName = "lecturer",
                PasswordHash = "hash",
                StudentCode = "LECT001",
                FullName = "Lecturer 1",
                Role = UserRole.Lecturer
            });

            dbContext.ExamRooms.Add(new ExamRoom
            {
                Id = roomId,
                Code = "R1",
                Name = "Room 1",
                LecturerId = lecturerId
            });

            dbContext.Exams.Add(new Exam
            {
                Id = examId,
                RoomId = roomId,
                Code = ExamCode,
                Title = ExamTitle,
                SolutionPattern = SolutionPattern
            });

            dbContext.Students.Add(new StudentAccount
            {
                Id = studentId,
                UserName = "student",
                PasswordHash = "hash",
                StudentCode = StudentCode,
                FullName = StudentName,
                Role = UserRole.Student
            });

            dbContext.Submissions.Add(new Submission
            {
                Id = submissionId,
                ExamId = examId,
                StudentAccountId = studentId,
                WorkspacePath = WorkspacePath,
                Status = SubmissionStatus.QueuedForGrading
            });

            await dbContext.SaveChangesAsync();
        }

        var consumer = new TestableGradingResultConsumer(
            provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new RabbitMqOptions()),
            new NotificationGrpcClient(Options.Create(new NotificationGrpcOptions()), NullLogger<NotificationGrpcClient>.Instance),
            NullLogger<GradingResultConsumer>.Instance);

        var rawReport = JsonSerializer.Serialize(new
        {
            sectionResults = new[]
            {
                new
                {
                    name = SectionName,
                    score = SectionScore,
                    maxScore = SectionMaxScore,
                    status = SectionStatus,
                    feedback = SectionFeedback
                }
            }
        });

        await consumer.ProcessForTestAsync(new SubmissionGradedEvent
        {
            SubmissionId = submissionId,
            ExamId = examId,
            StudentCode = StudentCode,
            TotalScore = TotalScore,
            RawJsonReport = rawReport
        });

        await using var verifyScope = provider.CreateAsyncScope();
        var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<ExamAccountDbContext>();
        var submission = await verifyDbContext.Submissions.SingleAsync();
        var sectionResult = await verifyDbContext.SubmissionSectionResults.SingleAsync();

        Assert.Equal(SubmissionStatus.GradedPendingPublication, submission.Status);
        Assert.Equal(TotalScore, submission.TotalScore);
        Assert.Equal(SectionName, sectionResult.SectionName);
        Assert.Equal(SectionScore, sectionResult.Score);
    }

    private sealed class TestableGradingResultConsumer : GradingResultConsumer
    {
        public TestableGradingResultConsumer(
            IServiceScopeFactory scopeFactory,
            IOptions<RabbitMqOptions> options,
            NotificationGrpcClient notificationGrpcClient,
            Microsoft.Extensions.Logging.ILogger<GradingResultConsumer> logger)
            : base(scopeFactory, options, notificationGrpcClient, logger)
        {
        }

        public Task ProcessForTestAsync(SubmissionGradedEvent message)
        {
            return InvokeProcessAsync(message);
        }
    }
}
