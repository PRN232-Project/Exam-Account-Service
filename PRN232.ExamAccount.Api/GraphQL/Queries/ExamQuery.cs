using HotChocolate;
using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Application.Dashboard;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Api.GraphQL.Queries;

public class ExamQuery
{
    public async Task<ExamDashboardDto?> GetExamDashboardAsync(
        Guid examId,
        [Service] ExamAccountDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return await dbContext.Exams
            .AsNoTracking()
            .Where(x => x.Id == examId)
            .Select(x => new ExamDashboardDto
            {
                ExamId = x.Id,
                ExamCode = x.Code,
                ExamTitle = x.Title,
                MaxScore = x.MaxScore,
                CandidateCount = x.Submissions.Count,
                Candidates = x.Submissions
                    .OrderByDescending(s => s.SubmittedAtUtc)
                    .Select(s => new CandidateDashboardDto
                    {
                        SubmissionId = s.Id,
                        StudentId = s.StudentAccountId,
                        StudentCode = s.StudentAccount!.StudentCode,
                        StudentName = s.StudentAccount!.FullName,
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
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
