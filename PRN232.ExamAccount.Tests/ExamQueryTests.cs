using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Api.GraphQL.Queries;
using PRN232.ExamAccount.Domain.Entities;
using PRN232.ExamAccount.Domain.Enums;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Tests;

public class ExamQueryTests
{
    private const string ExamCode = "PRN232-SU26";
    private const string ExamTitle = "PRN232 Final";
    private const string SolutionPattern = "*.sln";
    private const string SectionName = "CRUD";
    private const string SectionFilter = "Category=CRUD";
    private const decimal SectionWeight = 30m;
    private const string WorkspacePath = "workspace";
    private const string StudentCode = "SE0001";
    private const string StudentName = "Nguyen Van A";
    private const decimal TotalScore = 8m;
    private const decimal SectionScore = 3m;
    private const decimal SectionMaxScore = 4m;
    private const string SectionStatus = "Passed";

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
            Code = ExamCode,
            Title = ExamTitle,
            SolutionPattern = SolutionPattern,
            Sections =
            [
                new ExamSectionDefinition
                {
                    Id = Guid.NewGuid(),
                    Name = SectionName,
                    TestFilter = SectionFilter,
                    Weight = SectionWeight
                }
            ],
            Submissions =
            [
                new Submission
                {
                    Id = submissionId,
                    StudentAccountId = studentId,
                    WorkspacePath = WorkspacePath,
                    Status = SubmissionStatus.GradedPendingPublication,
                    TotalScore = TotalScore,
                    StudentAccount = new StudentAccount
                    {
                        Id = studentId,
                        StudentCode = StudentCode,
                        FullName = StudentName
                    },
                    SectionResults =
                    [
                        new SubmissionSectionResult
                        {
                            Id = Guid.NewGuid(),
                            SectionName = SectionName,
                            Score = SectionScore,
                            MaxScore = SectionMaxScore,
                            Status = SectionStatus
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
        Assert.Equal(StudentCode, result.Candidates[0].StudentCode);
        Assert.Single(result.Candidates[0].SectionResults);
    }
}
