using Microsoft.AspNetCore.Mvc;
using PRN232.ExamAccount.Application.Dashboard;

namespace PRN232.ExamAccount.Api.Controllers;

[ApiController]
[Route("api/exams")]
public class ExamsController : ControllerBase
{
    private readonly IExamDashboardReader _dashboardReader;

    public ExamsController(IExamDashboardReader dashboardReader)
    {
        _dashboardReader = dashboardReader;
    }

    [HttpGet("{examId:guid}/dashboard")]
    public async Task<ActionResult<ExamDashboardDto>> GetDashboardAsync(
        Guid examId,
        CancellationToken cancellationToken)
    {
        var dashboard = await _dashboardReader.GetExamDashboardAsync(examId, cancellationToken);
        return dashboard is null ? NotFound() : Ok(dashboard);
    }

    [HttpGet("{examId:guid}/submissions/{submissionId:guid}")]
    public async Task<ActionResult<CandidateDashboardDto>> GetSubmissionReportAsync(
        Guid examId,
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        var report = await _dashboardReader.GetSubmissionReportAsync(examId, submissionId, cancellationToken);
        return report is null ? NotFound() : Ok(report);
    }
}
