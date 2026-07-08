namespace PRN232.ExamAccount.Application.Dashboard;

public interface IExamDashboardReader
{
    Task<ExamDashboardDto?> GetExamDashboardAsync(Guid examId, CancellationToken cancellationToken = default);

    Task<CandidateDashboardDto?> GetSubmissionReportAsync(
        Guid examId,
        Guid submissionId,
        CancellationToken cancellationToken = default);
}
