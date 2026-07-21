using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Domain.Entities;
using PRN232.ExamAccount.Domain.Enums;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Api.Controllers;

[ApiController, Route("api/exam-papers"), Authorize(Roles = nameof(UserRole.ExamOfficer))]
public class ExamPapersController(ExamAccountDbContext db) : ControllerBase
{
    [HttpGet]
    public Task<List<ExamPaperDto>> GetAll(CancellationToken ct) => db.ExamPapers.AsNoTracking().Include(x => x.Sections)
        .OrderBy(x => x.Code).Select(x => MapProjection(x)).ToListAsync(ct);

    [HttpPost]
    public async Task<ActionResult<ExamPaperDto>> Create(UpsertExamPaperRequest r, CancellationToken ct)
    {
        if (await db.ExamPapers.AnyAsync(x => x.Code == r.Code.Trim(), ct)) return Conflict("Mã đề đã tồn tại.");
        var x = new ExamPaper { Id = Guid.NewGuid() }; Apply(x, r); db.Add(x); AddSections(x, r.Sections);
        await db.SaveChangesAsync(ct); return Created("api/exam-papers/" + x.Id, await Load(x.Id, ct));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExamPaperDto>> Update(Guid id, UpsertExamPaperRequest r, CancellationToken ct)
    {
        var x = await db.ExamPapers.Include(y => y.Sections).FirstOrDefaultAsync(y => y.Id == id, ct); if (x is null) return NotFound();
        if (await db.ExamPapers.AnyAsync(y => y.Id != id && y.Code == r.Code.Trim(), ct)) return Conflict("Mã đề đã tồn tại.");
        Apply(x, r); db.ExamSections.RemoveRange(x.Sections); AddSections(x, r.Sections); await db.SaveChangesAsync(ct); return Ok(await Load(id, ct));
    }

    private Task<ExamPaperDto> Load(Guid id, CancellationToken ct) => db.ExamPapers.AsNoTracking().Include(x => x.Sections).Where(x => x.Id == id).Select(x => MapProjection(x)).SingleAsync(ct);
    private static ExamPaperDto MapProjection(ExamPaper x) => new(x.Id, x.Code, x.Title, x.RubricVersion, x.MaxScore, x.SolutionPattern, x.RequireAppSettings, x.ForbidHardcodedConnectionString, x.TimeoutSeconds, x.PlagiarismKeywords, x.IsActive, x.Sections.OrderBy(s => s.Name).Select(s => new ExamSectionDto(s.Id, s.Name, s.Weight, s.TestFilter)).ToList());
    private static void Apply(ExamPaper x, UpsertExamPaperRequest r) { x.Code = r.Code.Trim(); x.Title = r.Title.Trim(); x.RubricVersion = r.RubricVersion.Trim(); x.MaxScore = r.MaxScore; x.SolutionPattern = r.SolutionPattern.Trim(); x.RequireAppSettings = r.RequireAppSettings; x.ForbidHardcodedConnectionString = r.ForbidHardcodedConnectionString; x.TimeoutSeconds = r.TimeoutSeconds; x.PlagiarismKeywords = r.PlagiarismKeywords ?? []; x.IsActive = r.IsActive; }
    private static void AddSections(ExamPaper x, IReadOnlyList<UpsertExamSectionRequest> sections) { foreach (var s in sections) x.Sections.Add(new ExamSectionDefinition { Id = Guid.NewGuid(), ExamPaperId = x.Id, Name = s.Name.Trim(), Weight = s.Weight, TestFilter = s.TestFilter.Trim() }); }
}
public record UpsertExamSectionRequest(string Name, decimal Weight, string TestFilter);
public record UpsertExamPaperRequest(string Code, string Title, string RubricVersion, decimal MaxScore, string SolutionPattern, bool RequireAppSettings, bool ForbidHardcodedConnectionString, int TimeoutSeconds, string[] PlagiarismKeywords, IReadOnlyList<UpsertExamSectionRequest> Sections, bool IsActive = true);
public record ExamSectionDto(Guid Id, string Name, decimal Weight, string TestFilter);
public record ExamPaperDto(Guid Id, string Code, string Title, string RubricVersion, decimal MaxScore, string SolutionPattern, bool RequireAppSettings, bool ForbidHardcodedConnectionString, int TimeoutSeconds, string[] PlagiarismKeywords, bool IsActive, IReadOnlyList<ExamSectionDto> Sections);
