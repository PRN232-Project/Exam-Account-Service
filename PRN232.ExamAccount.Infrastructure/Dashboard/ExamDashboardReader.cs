using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Application.Dashboard;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Infrastructure.Dashboard;

public class ExamDashboardReader : IExamDashboardReader
{
    private readonly ExamAccountDbContext _dbContext;

    public ExamDashboardReader(ExamAccountDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExamDashboardDto?> GetExamDashboardAsync(Guid examId, CancellationToken cancellationToken = default)
    {
        var exam = await _dbContext.Exams
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Submissions)
                .ThenInclude(x => x.StudentAccount)
            .Include(x => x.Submissions)
                .ThenInclude(x => x.SectionResults)
            .FirstOrDefaultAsync(x => x.Id == examId, cancellationToken);

        return exam is null
            ? null
            : new ExamDashboardDto
            {
                ExamId = exam.Id,
                ExamCode = exam.Code,
                ExamTitle = exam.Title,
                MaxScore = exam.MaxScore,
                CandidateCount = exam.Submissions.Count,
                Candidates = exam.Submissions
                    .OrderByDescending(s => s.SubmittedAtUtc)
                    .Select(MapCandidate)
                    .ToList()
            };
    }

    public async Task<CandidateDashboardDto?> GetSubmissionReportAsync(
        Guid examId,
        Guid submissionId,
        CancellationToken cancellationToken = default)
    {
        var submission = await _dbContext.Submissions
            .AsNoTracking()
            .Include(x => x.StudentAccount)
            .Include(x => x.SectionResults)
            .FirstOrDefaultAsync(
                x => x.ExamId == examId && x.Id == submissionId,
                cancellationToken);

        return submission is null ? null : MapCandidate(submission);
    }

    private static CandidateDashboardDto MapCandidate(Domain.Entities.Submission submission)
    {
        return new CandidateDashboardDto
        {
            SubmissionId = submission.Id,
            StudentId = submission.StudentAccountId,
            StudentCode = submission.StudentAccount?.StudentCode ?? string.Empty,
            StudentName = submission.StudentAccount?.FullName ?? string.Empty,
            Status = submission.Status.ToString(),
            TotalScore = submission.TotalScore,
            SubmittedAtUtc = submission.SubmittedAtUtc,
            GradedAtUtc = submission.GradedAtUtc,
            SectionResults = submission.SectionResults
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
        };
    }
}
