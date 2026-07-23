using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Domain.Entities;
using PRN232.ExamAccount.Domain.Enums;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Api.Controllers;

[ApiController, Route("api/exam-sessions"), Authorize(Roles = nameof(UserRole.ExamOfficer))]
public class ExamSessionsController(ExamAccountDbContext db) : ControllerBase
{
    [HttpGet]
    public Task<List<ExamSessionDto>> GetAll(CancellationToken ct) => db.ExamSessions.AsNoTracking().Include(x => x.Room).Include(x => x.ExamPaper).Include(x => x.Candidates)
        .OrderByDescending(x => x.ScheduledAtUtc).Select(x => new ExamSessionDto(x.Id, x.Code, x.Title, x.RoomId, x.Room!.Code, x.ExamPaperId, x.ExamPaper!.Code, x.ScheduledAtUtc, x.Status, x.Candidates.Count)).ToListAsync(ct);

    [HttpPost]
    public async Task<ActionResult<ExamSessionDto>> Create(CreateExamSessionRequest r, CancellationToken ct)
    {
        if (await db.ExamSessions.AnyAsync(x => x.Code == r.Code.Trim(), ct)) return Conflict("Mã ca thi đã tồn tại.");
        if (!await db.ExamRooms.AnyAsync(x => x.Id == r.RoomId && x.IsActive, ct) || !await db.ExamPapers.AnyAsync(x => x.Id == r.ExamPaperId && x.IsActive, ct)) return BadRequest("Phòng hoặc mã đề không hợp lệ.");
        var x = new ExamSession { Id = Guid.NewGuid(), Code = r.Code.Trim(), Title = r.Title.Trim(), RoomId = r.RoomId, ExamPaperId = r.ExamPaperId, ScheduledAtUtc = r.ScheduledAtUtc, Status = ExamSessionStatus.Draft };
        db.Add(x); await db.SaveChangesAsync(ct); return Created("api/exam-sessions/" + x.Id, new { x.Id, x.Code, x.Status });
    }

    [HttpGet("{id:guid}/candidates")]
    public async Task<ActionResult<IReadOnlyList<CandidateDto>>> GetCandidates(Guid id, CancellationToken ct)
    {
        if (!await db.ExamSessions.AnyAsync(x => x.Id == id, ct)) return NotFound();
        return Ok(await db.ExamCandidates.AsNoTracking().Where(x => x.ExamSessionId == id).OrderBy(x => x.Student!.StudentCode)
            .Select(x => new CandidateDto(x.Id, x.StudentId, x.Student!.StudentCode, x.Student.FullName, x.PaperCode, x.IsAbsent, x.GradingItem != null)).ToListAsync(ct));
    }

    [HttpPost("{id:guid}/candidates")]
    public async Task<ActionResult<object>> AddCandidates(Guid id, AddCandidatesRequest r, CancellationToken ct)
    {
        var session = await db.ExamSessions.Include(x => x.ExamPaper).Include(x => x.Candidates).FirstOrDefaultAsync(x => x.Id == id, ct); if (session is null) return NotFound();
        if (session.Status != ExamSessionStatus.Draft) return BadRequest("Chỉ được sửa danh sách khi ca thi ở Draft.");
        var ids = r.StudentIds.Distinct().ToList(); var students = await db.Students.Where(x => ids.Contains(x.Id) && x.IsActive).Select(x => x.Id).ToListAsync(ct);
        if (students.Count != ids.Count) return BadRequest("Danh sách có sinh viên không tồn tại hoặc đã bị khoá.");
        var existing = session.Candidates.Select(x => x.StudentId).ToHashSet();
        foreach (var studentId in students.Where(x => !existing.Contains(x)))
            db.ExamCandidates.Add(new ExamCandidate { Id = Guid.NewGuid(), StudentId = studentId, ExamSessionId = id, PaperCode = session.ExamPaper!.Code });
        await db.SaveChangesAsync(ct); return Ok(new { candidateCount = session.Candidates.Count });
    }

    [HttpPost("{id:guid}/ready")]
    public async Task<IActionResult> Ready(Guid id, CancellationToken ct) { var x = await db.ExamSessions.Include(y => y.Candidates).FirstOrDefaultAsync(y => y.Id == id, ct); if (x is null) return NotFound(); if (x.Status != ExamSessionStatus.Draft || x.Candidates.Count == 0) return BadRequest("Ca thi phải ở Draft và có sinh viên."); x.Status = ExamSessionStatus.Ready; await db.SaveChangesAsync(ct); return Ok(new { x.Id, x.Status }); }
}
public record CreateExamSessionRequest(string Code, string Title, Guid RoomId, Guid ExamPaperId, DateTime ScheduledAtUtc);
public record AddCandidatesRequest(IReadOnlyList<Guid> StudentIds);
public record ExamSessionDto(Guid Id, string Code, string Title, Guid RoomId, string RoomCode, Guid ExamPaperId, string ExamPaperCode, DateTime ScheduledAtUtc, ExamSessionStatus Status, int CandidateCount);
public record CandidateDto(Guid Id, Guid StudentId, string StudentCode, string StudentName, string PaperCode, bool IsAbsent, bool IsAssigned);
