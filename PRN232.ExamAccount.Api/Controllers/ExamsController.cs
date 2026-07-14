using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Api.Security;
using PRN232.ExamAccount.Application.Dashboard;
using PRN232.ExamAccount.Domain.Enums;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Api.Controllers;

[ApiController]
[Route("api/exams")]
public class ExamsController : ControllerBase
{
    private readonly IExamDashboardReader _dashboardReader;
    private readonly ExamAccountDbContext _dbContext;

    public ExamsController(IExamDashboardReader dashboardReader, ExamAccountDbContext dbContext)
    {
        _dashboardReader = dashboardReader;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExamListItemDto>>> GetExamsAsync(CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin, UserRole.Lecturer, UserRole.Student);
        if (auth.Error is not null) return auth.Error;

        var query = _dbContext.Exams
            .AsNoTracking()
            .Include(x => x.Room)
            .AsQueryable();

        if (auth.User!.Role == UserRole.Lecturer)
        {
            query = query.Where(x => x.Room != null && x.Room.LecturerId == auth.User.Id);
        }

        var exams = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new ExamListItemDto
            {
                ExamId = x.Id,
                Code = x.Code,
                Title = x.Title,
                RoomId = x.RoomId,
                MaxScore = x.MaxScore,
                SolutionPattern = x.SolutionPattern,
                RequireAppSettings = x.RequireAppSettings,
                ForbidHardcodedConnectionString = x.ForbidHardcodedConnectionString,
                TimeoutSeconds = x.TimeoutSeconds,
                PlagiarismKeywords = x.PlagiarismKeywords
            })
            .ToListAsync(cancellationToken);

        return Ok(exams);
    }

    [HttpGet("{examId:guid}")]
    public async Task<ActionResult<ExamListItemDto>> GetExamByIdAsync(Guid examId, CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin, UserRole.Lecturer, UserRole.Student);
        if (auth.Error is not null) return auth.Error;

        var exam = await _dbContext.Exams
            .AsNoTracking()
            .Include(x => x.Room)
            .FirstOrDefaultAsync(x => x.Id == examId, cancellationToken);

        if (exam is null)
        {
            return NotFound();
        }

        if (auth.User!.Role == UserRole.Lecturer && exam.Room?.LecturerId != auth.User.Id)
        {
            return Forbid();
        }

        return Ok(MapExam(exam));
    }

    [HttpPost]
    public async Task<ActionResult<ExamListItemDto>> CreateExamAsync([FromBody] CreateExamRequest request, CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin);
        if (auth.Error is not null) return auth.Error;

        var roomExists = await _dbContext.ExamRooms.AnyAsync(x => x.Id == request.RoomId, cancellationToken);
        if (!roomExists)
        {
            return BadRequest("RoomId khong ton tai.");
        }

        var exam = new Domain.Entities.Exam
        {
            Id = Guid.NewGuid(),
            RoomId = request.RoomId,
            Code = request.Code.Trim(),
            Title = request.Title.Trim(),
            MaxScore = request.MaxScore,
            SolutionPattern = request.SolutionPattern.Trim(),
            RequireAppSettings = request.RequireAppSettings,
            ForbidHardcodedConnectionString = request.ForbidHardcodedConnectionString,
            TimeoutSeconds = request.TimeoutSeconds,
            PlagiarismKeywords = request.PlagiarismKeywords
        };

        _dbContext.Exams.Add(exam);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(MapExam(exam));
    }

    [HttpPut("{examId:guid}")]
    public async Task<ActionResult<ExamListItemDto>> UpdateExamAsync(Guid examId, [FromBody] CreateExamRequest request, CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin);
        if (auth.Error is not null) return auth.Error;

        var exam = await _dbContext.Exams.FirstOrDefaultAsync(x => x.Id == examId, cancellationToken);
        if (exam is null)
        {
            return NotFound();
        }

        var roomExists = await _dbContext.ExamRooms.AnyAsync(x => x.Id == request.RoomId, cancellationToken);
        if (!roomExists)
        {
            return BadRequest("RoomId khong ton tai.");
        }

        exam.RoomId = request.RoomId;
        exam.Code = request.Code.Trim();
        exam.Title = request.Title.Trim();
        exam.MaxScore = request.MaxScore;
        exam.SolutionPattern = request.SolutionPattern.Trim();
        exam.RequireAppSettings = request.RequireAppSettings;
        exam.ForbidHardcodedConnectionString = request.ForbidHardcodedConnectionString;
        exam.TimeoutSeconds = request.TimeoutSeconds;
        exam.PlagiarismKeywords = request.PlagiarismKeywords;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(MapExam(exam));
    }

    [HttpDelete("{examId:guid}")]
    public async Task<IActionResult> DeleteExamAsync(Guid examId, CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin);
        if (auth.Error is not null) return auth.Error;

        var exam = await _dbContext.Exams
            .Include(x => x.Submissions)
            .Include(x => x.Sections)
            .FirstOrDefaultAsync(x => x.Id == examId, cancellationToken);

        if (exam is null)
        {
            return NotFound();
        }

        if (exam.Submissions.Count > 0)
        {
            return BadRequest("Khong the xoa exam da co submission.");
        }

        if (exam.Sections.Count > 0)
        {
            _dbContext.ExamSections.RemoveRange(exam.Sections);
        }

        _dbContext.Exams.Remove(exam);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("{examId:guid}/dashboard")]
    public async Task<ActionResult<ExamDashboardDto>> GetDashboardAsync(
        Guid examId,
        CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin, UserRole.Lecturer);
        if (auth.Error is not null) return auth.Error;

        if (auth.User!.Role == UserRole.Lecturer)
        {
            var allowed = await _dbContext.Exams.AnyAsync(
                x => x.Id == examId && x.Room != null && x.Room.LecturerId == auth.User.Id,
                cancellationToken);

            if (!allowed)
            {
                return Forbid();
            }
        }

        var dashboard = await _dashboardReader.GetExamDashboardAsync(examId, cancellationToken);
        return dashboard is null ? NotFound() : Ok(dashboard);
    }

    [HttpGet("{examId:guid}/submissions/{submissionId:guid}")]
    public async Task<ActionResult<CandidateDashboardDto>> GetSubmissionReportAsync(
        Guid examId,
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin, UserRole.Lecturer);
        if (auth.Error is not null) return auth.Error;

        if (auth.User!.Role == UserRole.Lecturer)
        {
            var allowed = await _dbContext.Exams.AnyAsync(
                x => x.Id == examId && x.Room != null && x.Room.LecturerId == auth.User.Id,
                cancellationToken);

            if (!allowed)
            {
                return Forbid();
            }
        }

        var report = await _dashboardReader.GetSubmissionReportAsync(examId, submissionId, cancellationToken);
        return report is null ? NotFound() : Ok(report);
    }

    private static ExamListItemDto MapExam(Domain.Entities.Exam exam)
    {
        return new ExamListItemDto
        {
            ExamId = exam.Id,
            Code = exam.Code,
            Title = exam.Title,
            RoomId = exam.RoomId,
            MaxScore = exam.MaxScore,
            SolutionPattern = exam.SolutionPattern,
            RequireAppSettings = exam.RequireAppSettings,
            ForbidHardcodedConnectionString = exam.ForbidHardcodedConnectionString,
            TimeoutSeconds = exam.TimeoutSeconds,
            PlagiarismKeywords = exam.PlagiarismKeywords
        };
    }
}

public class ExamListItemDto
{
    public Guid ExamId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public Guid RoomId { get; set; }
    public decimal MaxScore { get; set; }
    public string SolutionPattern { get; set; } = string.Empty;
    public bool RequireAppSettings { get; set; }
    public bool ForbidHardcodedConnectionString { get; set; }
    public int TimeoutSeconds { get; set; }
    public string[] PlagiarismKeywords { get; set; } = [];
}

public class CreateExamRequest
{
    public Guid RoomId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal MaxScore { get; set; } = 10m;
    public string SolutionPattern { get; set; } = string.Empty;
    public bool RequireAppSettings { get; set; } = true;
    public bool ForbidHardcodedConnectionString { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 15;
    public string[] PlagiarismKeywords { get; set; } = [];
}
