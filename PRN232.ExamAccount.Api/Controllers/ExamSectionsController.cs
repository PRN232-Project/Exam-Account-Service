using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Api.Security;
using PRN232.ExamAccount.Domain.Entities;
using PRN232.ExamAccount.Domain.Enums;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Api.Controllers;

[ApiController]
[Route("api/exam-sections")]
public class ExamSectionsController : ControllerBase
{
    private readonly ExamAccountDbContext _dbContext;

    public ExamSectionsController(ExamAccountDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExamSectionDto>>> GetSectionsAsync([FromQuery] Guid examId, CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin, UserRole.Lecturer, UserRole.Student);
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

        var sections = await _dbContext.ExamSections
            .AsNoTracking()
            .Where(x => x.ExamId == examId)
            .OrderBy(x => x.Name)
            .Select(x => new ExamSectionDto
            {
                Id = x.Id,
                ExamId = x.ExamId,
                Name = x.Name,
                Weight = x.Weight,
                TestFilter = x.TestFilter
            })
            .ToListAsync(cancellationToken);

        return Ok(sections);
    }

    [HttpGet("{sectionId:guid}")]
    public async Task<ActionResult<ExamSectionDto>> GetSectionByIdAsync(Guid sectionId, CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin, UserRole.Lecturer, UserRole.Student);
        if (auth.Error is not null) return auth.Error;

        var section = await _dbContext.ExamSections
            .AsNoTracking()
            .Include(x => x.Exam)
                .ThenInclude(x => x!.Room)
            .FirstOrDefaultAsync(x => x.Id == sectionId, cancellationToken);

        if (section is null)
        {
            return NotFound();
        }

        if (auth.User!.Role == UserRole.Lecturer && section.Exam?.Room?.LecturerId != auth.User.Id)
        {
            return Forbid();
        }

        return Ok(MapSection(section));
    }

    [HttpPost]
    public async Task<ActionResult<ExamSectionDto>> CreateSectionAsync([FromBody] UpsertExamSectionRequest request, CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin);
        if (auth.Error is not null) return auth.Error;

        var examExists = await _dbContext.Exams.AnyAsync(x => x.Id == request.ExamId, cancellationToken);
        if (!examExists)
        {
            return BadRequest("ExamId khong ton tai.");
        }

        var section = new ExamSectionDefinition
        {
            Id = Guid.NewGuid(),
            ExamId = request.ExamId,
            Name = request.Name.Trim(),
            Weight = request.Weight,
            TestFilter = request.TestFilter.Trim()
        };

        _dbContext.ExamSections.Add(section);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(MapSection(section));
    }

    [HttpPut("{sectionId:guid}")]
    public async Task<ActionResult<ExamSectionDto>> UpdateSectionAsync(Guid sectionId, [FromBody] UpsertExamSectionRequest request, CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin);
        if (auth.Error is not null) return auth.Error;

        var section = await _dbContext.ExamSections.FirstOrDefaultAsync(x => x.Id == sectionId, cancellationToken);
        if (section is null)
        {
            return NotFound();
        }

        var examExists = await _dbContext.Exams.AnyAsync(x => x.Id == request.ExamId, cancellationToken);
        if (!examExists)
        {
            return BadRequest("ExamId khong ton tai.");
        }

        section.ExamId = request.ExamId;
        section.Name = request.Name.Trim();
        section.Weight = request.Weight;
        section.TestFilter = request.TestFilter.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(MapSection(section));
    }

    [HttpDelete("{sectionId:guid}")]
    public async Task<IActionResult> DeleteSectionAsync(Guid sectionId, CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin);
        if (auth.Error is not null) return auth.Error;

        var section = await _dbContext.ExamSections.FirstOrDefaultAsync(x => x.Id == sectionId, cancellationToken);
        if (section is null)
        {
            return NotFound();
        }

        _dbContext.ExamSections.Remove(section);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static ExamSectionDto MapSection(ExamSectionDefinition section)
    {
        return new ExamSectionDto
        {
            Id = section.Id,
            ExamId = section.ExamId,
            Name = section.Name,
            Weight = section.Weight,
            TestFilter = section.TestFilter
        };
    }
}

public class UpsertExamSectionRequest
{
    public Guid ExamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string TestFilter { get; set; } = string.Empty;
}

public class ExamSectionDto
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string TestFilter { get; set; } = string.Empty;
}
