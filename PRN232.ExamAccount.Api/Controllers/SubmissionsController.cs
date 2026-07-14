using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Api.Security;
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
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Student);
        if (auth.Error is not null) return auth.Error;
        if (auth.User!.Id != request.StudentId)
        {
            return Forbid();
        }

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

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SubmissionListItemDto>>> GetSubmissionsAsync(
        [FromQuery] Guid? studentId,
        CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin, UserRole.Lecturer, UserRole.Student);
        if (auth.Error is not null) return auth.Error;

        var query = _dbContext.Submissions
            .AsNoTracking()
            .Include(x => x.StudentAccount)
            .Include(x => x.Exam)
                .ThenInclude(x => x!.Room)
            .AsQueryable();

        if (auth.User!.Role == UserRole.Student)
        {
            query = query.Where(x => x.StudentAccountId == auth.User.Id);
        }
        else if (auth.User.Role == UserRole.Lecturer)
        {
            query = query.Where(x => x.Exam != null && x.Exam.Room != null && x.Exam.Room.LecturerId == auth.User.Id);
        }
        else if (studentId.HasValue)
        {
            query = query.Where(x => x.StudentAccountId == studentId.Value);
        }

        var data = await query
            .OrderByDescending(x => x.SubmittedAtUtc)
            .Select(x => new SubmissionListItemDto
            {
                SubmissionId = x.Id,
                ExamId = x.ExamId,
                StudentId = x.StudentAccountId,
                StudentCode = x.StudentAccount != null ? x.StudentAccount.StudentCode : string.Empty,
                Status = x.Status.ToString(),
                TotalScore = x.TotalScore,
                ErrorMessage = x.ErrorMessage,
                SubmittedAtUtc = x.SubmittedAtUtc,
                GradedAtUtc = x.GradedAtUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(data);
    }

    [HttpGet("{submissionId:guid}")]
    public async Task<ActionResult<SubmissionListItemDto>> GetSubmissionByIdAsync(Guid submissionId, CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin, UserRole.Lecturer, UserRole.Student);
        if (auth.Error is not null) return auth.Error;

        var submission = await _dbContext.Submissions
            .AsNoTracking()
            .Include(x => x.StudentAccount)
            .Include(x => x.Exam)
                .ThenInclude(x => x!.Room)
            .FirstOrDefaultAsync(x => x.Id == submissionId, cancellationToken);

        if (submission is null)
        {
            return NotFound();
        }

        if (auth.User!.Role == UserRole.Student && submission.StudentAccountId != auth.User.Id)
        {
            return Forbid();
        }

        if (auth.User.Role == UserRole.Lecturer && submission.Exam?.Room?.LecturerId != auth.User.Id)
        {
            return Forbid();
        }

        return Ok(MapSubmission(submission));
    }

    [HttpPost("{submissionId:guid}/regrade")]
    public async Task<ActionResult<SubmissionResponse>> RegradeAsync(
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin, UserRole.Lecturer);
        if (auth.Error is not null) return auth.Error;

        var submission = await _dbContext.Submissions
            .Include(x => x.Exam)
                .ThenInclude(x => x!.Room)
            .Include(x => x.SectionResults)
            .FirstOrDefaultAsync(x => x.Id == submissionId, cancellationToken);

        if (submission is null)
        {
            return NotFound();
        }

        if (auth.User!.Role == UserRole.Lecturer && submission.Exam?.Room?.LecturerId != auth.User.Id)
        {
            return Forbid();
        }

        submission.TotalScore = null;
        submission.RawJsonReport = string.Empty;
        submission.ErrorMessage = string.Empty;
        submission.GradedAtUtc = null;
        submission.Status = SubmissionStatus.RegradingRequested;
        submission.SectionResults.Clear();

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _gradingJobPublisher.PublishSubmissionAsync(submissionId, isRegrade: true, cancellationToken);

        return Accepted(new SubmissionResponse(submissionId, SubmissionStatus.RegradingRequested.ToString()));
    }

    [HttpDelete("{submissionId:guid}")]
    public async Task<IActionResult> DeleteSubmissionAsync(Guid submissionId, CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin, UserRole.Lecturer);
        if (auth.Error is not null) return auth.Error;

        var submission = await _dbContext.Submissions
            .Include(x => x.Exam)
                .ThenInclude(x => x!.Room)
            .Include(x => x.SectionResults)
            .FirstOrDefaultAsync(x => x.Id == submissionId, cancellationToken);

        if (submission is null)
        {
            return NotFound();
        }

        if (auth.User!.Role == UserRole.Lecturer && submission.Exam?.Room?.LecturerId != auth.User.Id)
        {
            return Forbid();
        }

        if (submission.SectionResults.Count > 0)
        {
            _dbContext.SubmissionSectionResults.RemoveRange(submission.SectionResults);
        }

        _dbContext.Submissions.Remove(submission);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static SubmissionListItemDto MapSubmission(Submission submission)
    {
        return new SubmissionListItemDto
        {
            SubmissionId = submission.Id,
            ExamId = submission.ExamId,
            StudentId = submission.StudentAccountId,
            StudentCode = submission.StudentAccount?.StudentCode ?? string.Empty,
            Status = submission.Status.ToString(),
            TotalScore = submission.TotalScore,
            ErrorMessage = submission.ErrorMessage,
            SubmittedAtUtc = submission.SubmittedAtUtc,
            GradedAtUtc = submission.GradedAtUtc
        };
    }
}

public record CreateSubmissionRequest(Guid ExamId, Guid StudentId, string WorkspacePath);

public record SubmissionResponse(Guid SubmissionId, string Status);

public class SubmissionListItemDto
{
    public Guid SubmissionId { get; set; }
    public Guid ExamId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal? TotalScore { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime SubmittedAtUtc { get; set; }
    public DateTime? GradedAtUtc { get; set; }
}
