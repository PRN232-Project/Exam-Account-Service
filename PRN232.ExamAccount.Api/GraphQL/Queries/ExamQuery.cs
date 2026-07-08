using HotChocolate;
using PRN232.ExamAccount.Application.Dashboard;

namespace PRN232.ExamAccount.Api.GraphQL.Queries;

public class ExamQuery
{
    [GraphQLName("examDashboard")]
    public async Task<ExamDashboardDto?> GetExamDashboardAsync(
        Guid examId,
        [Service] IExamDashboardReader dashboardReader,
        CancellationToken cancellationToken)
    {
        return await dashboardReader.GetExamDashboardAsync(examId, cancellationToken);
    }

    [GraphQLName("submissionReport")]
    public async Task<CandidateDashboardDto?> GetSubmissionReportAsync(
        Guid examId,
        Guid submissionId,
        [Service] IExamDashboardReader dashboardReader,
        CancellationToken cancellationToken)
    {
        return await dashboardReader.GetSubmissionReportAsync(examId, submissionId, cancellationToken);
    }
}
