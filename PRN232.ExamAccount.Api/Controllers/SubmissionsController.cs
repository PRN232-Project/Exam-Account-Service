using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Application.Interfaces;
using PRN232.ExamAccount.Domain.Entities;
using PRN232.ExamAccount.Domain.Enums;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubmissionsController : ControllerBase
{
    private readonly ExamAccountDbContext _dbContext;
    private readonly IGradingJobPublisher _gradingJobPublisher;

    public SubmissionsController(
        ExamAccountDbContext dbContext,
        IGradingJobPublisher gradingJobPublisher)
    {
        _dbContext = dbContext;
        _gradingJobPublisher = gradingJobPublisher;
    }

    [HttpPost]
    public async Task<ActionResult<SubmissionResponse>> SubmitAsync(
        [FromBody] CreateSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        var examExists = await _dbContext.Exams.AnyAsync(x => x.Id == request.ExamId, cancellationToken);
        var studentExists = await _dbContext.Students.AnyAsync(x => x.Id == request.StudentId, cancellationToken);

        if (!examExists || !studentExists)
        {
            return BadRequest("Exam hoặc Student không tồn tại.");
        }

        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            ExamId = request.ExamId,
            StudentAccountId = request.StudentId,
            WorkspacePath = request.WorkspacePath,
            Status = SubmissionStatus.Submitted,
            SubmittedAtUtc = DateTime.UtcNow
        };

        _dbContext.Submissions.Add(submission);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _gradingJobPublisher.PublishSubmissionAsync(submission.Id, isRegrade: false, cancellationToken);

        return Accepted(new SubmissionResponse(submission.Id, SubmissionStatus.QueuedForGrading.ToString()));
    }

    [HttpPost("{submissionId:guid}/regrade")]
    public async Task<ActionResult<SubmissionResponse>> RegradeAsync(
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        var submission = await _dbContext.Submissions
            .Include(x => x.SectionResults)
            .FirstOrDefaultAsync(x => x.Id == submissionId, cancellationToken);

        if (submission is null)
        {
            return NotFound();
        }

        submission.TotalScore = null;
        submission.RawJsonReport = string.Empty;
        submission.GradedAtUtc = null;
        submission.Status = SubmissionStatus.RegradingRequested;
        submission.SectionResults.Clear();

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _gradingJobPublisher.PublishSubmissionAsync(submissionId, isRegrade: true, cancellationToken);

        return Accepted(new SubmissionResponse(submissionId, SubmissionStatus.RegradingRequested.ToString()));
    }
}

public record CreateSubmissionRequest(Guid ExamId, Guid StudentId, string WorkspacePath);

public record SubmissionResponse(Guid SubmissionId, string Status);
