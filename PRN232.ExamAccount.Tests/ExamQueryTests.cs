using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Api.GraphQL.Queries;
using PRN232.ExamAccount.Domain.Entities;
using PRN232.ExamAccount.Domain.Enums;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Tests;

public class ExamQueryTests
{
    [Fact]
    public async Task GetExamDashboardAsync_ReturnsNestedCandidateTree()
    {
        var options = new DbContextOptionsBuilder<ExamAccountDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new ExamAccountDbContext(options);
        var examId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var submissionId = Guid.NewGuid();

        dbContext.Exams.Add(new Exam
        {
            Id = examId,
            Code = "PRN232-SU26",
            Title = "PRN232 Final",
            SolutionPattern = "*.sln",
            Sections =
            [
                new ExamSectionDefinition { Id = Guid.NewGuid(), Name = "CRUD", TestFilter = "Category=CRUD", Weight = 30 }
            ],
            Submissions =
            [
                new Submission
                {
                    Id = submissionId,
                    StudentAccountId = studentId,
                    WorkspacePath = "workspace",
                    Status = SubmissionStatus.GradedPendingPublication,
                    TotalScore = 8m,
                    StudentAccount = new StudentAccount
                    {
                        Id = studentId,
                        StudentCode = "SE0001",
                        FullName = "Nguyen Van A"
                    },
                    SectionResults =
                    [
                        new SubmissionSectionResult
                        {
                            Id = Guid.NewGuid(),
                            SectionName = "CRUD",
                            Score = 3m,
                            MaxScore = 4m,
                            Status = "Passed"
                        }
                    ]
                }
            ]
        });

        await dbContext.SaveChangesAsync();

        var query = new ExamQuery();
        var result = await query.GetExamDashboardAsync(examId, dbContext, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(examId, result!.ExamId);
        Assert.Single(result.Candidates);
        Assert.Equal("SE0001", result.Candidates[0].StudentCode);
        Assert.Single(result.Candidates[0].SectionResults);
    }
}
