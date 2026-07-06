using HotChocolate;
using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Application.Dashboard;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Api.GraphQL.Queries;

public class ExamQuery
{
    [GraphQLName("examDashboard")]
    public async Task<ExamDashboardDto?> GetExamDashboardAsync(
        Guid examId,
        [Service] ExamAccountDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var exam = await dbContext.Exams
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Submissions)
                .ThenInclude(x => x.StudentAccount)
            .Include(x => x.Submissions)
                .ThenInclude(x => x.SectionResults)
            .FirstOrDefaultAsync(x => x.Id == examId, cancellationToken);

        if (exam is null)
        {
            return null;
        }

        return new ExamDashboardDto
        {
            ExamId = exam.Id,
            ExamCode = exam.Code,
            ExamTitle = exam.Title,
            MaxScore = exam.MaxScore,
            CandidateCount = exam.Submissions.Count,
            Candidates = exam.Submissions
                .OrderByDescending(s => s.SubmittedAtUtc)
                .Select(s => new CandidateDashboardDto
                {
                    SubmissionId = s.Id,
                    StudentId = s.StudentAccountId,
                    StudentCode = s.StudentAccount?.StudentCode ?? string.Empty,
                    StudentName = s.StudentAccount?.FullName ?? string.Empty,
                    Status = s.Status.ToString(),
                    TotalScore = s.TotalScore,
                    SubmittedAtUtc = s.SubmittedAtUtc,
                    GradedAtUtc = s.GradedAtUtc,
                    SectionResults = s.SectionResults
                        .OrderBy(r => r.SectionName)
                        .Select(r => new SectionScoreDto
                        {
                            SectionName = r.SectionName,
                            Score = r.Score,
                            MaxScore = r.MaxScore,
                            Status = r.Status,
                            Feedback = r.Feedback
                        })
                        .ToList()
                })
                .ToList()
        };
    }
}
