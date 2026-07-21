using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Domain.Entities;
using PRN232.ExamAccount.Domain.Enums;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Api.Controllers;

[ApiController, Route("api/students"), Authorize(Roles = nameof(UserRole.ExamOfficer))]
public class StudentsController(ExamAccountDbContext db) : ControllerBase
{
    [HttpGet] public Task<List<StudentDto>> GetAll(string? search, CancellationToken ct) { var q = db.Students.AsNoTracking().AsQueryable(); if (!string.IsNullOrWhiteSpace(search)) q = q.Where(x => x.StudentCode.Contains(search) || x.FullName.Contains(search)); return q.OrderBy(x => x.StudentCode).Select(x => new StudentDto(x.Id, x.StudentCode, x.FullName, x.Email, x.ClassName, x.IsActive)).ToListAsync(ct); }
    [HttpPost] public async Task<ActionResult<StudentDto>> Create(UpsertStudentRequest r, CancellationToken ct) { if (await db.Students.AnyAsync(x => x.StudentCode == r.StudentCode.Trim(), ct)) return Conflict("Mã sinh viên đã tồn tại."); var x = new Student { Id = Guid.NewGuid(), StudentCode = r.StudentCode.Trim(), FullName = r.FullName.Trim(), Email = r.Email.Trim(), ClassName = r.ClassName.Trim(), IsActive = r.IsActive }; db.Add(x); await db.SaveChangesAsync(ct); return Created("api/students/" + x.Id, Map(x)); }
    [HttpPost("import")] public async Task<ActionResult<object>> Import(List<UpsertStudentRequest> rows, CancellationToken ct) { var codes = rows.Select(x => x.StudentCode.Trim()).Where(x => x.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList(); var existing = await db.Students.Where(x => codes.Contains(x.StudentCode)).ToDictionaryAsync(x => x.StudentCode, StringComparer.OrdinalIgnoreCase, ct); foreach (var row in rows.Where(x => !string.IsNullOrWhiteSpace(x.StudentCode))) { var code = row.StudentCode.Trim(); if (!existing.TryGetValue(code, out var x)) { x = new Student { Id = Guid.NewGuid(), StudentCode = code }; db.Add(x); existing[code] = x; } x.FullName = row.FullName.Trim(); x.Email = row.Email.Trim(); x.ClassName = row.ClassName.Trim(); x.IsActive = row.IsActive; } await db.SaveChangesAsync(ct); return Ok(new { imported = existing.Count }); }
    [HttpPut("{id:guid}")] public async Task<ActionResult<StudentDto>> Update(Guid id, UpsertStudentRequest r, CancellationToken ct) { var x = await db.Students.FindAsync([id], ct); if (x is null) return NotFound(); if (await db.Students.AnyAsync(y => y.Id != id && y.StudentCode == r.StudentCode.Trim(), ct)) return Conflict("Mã sinh viên đã tồn tại."); x.StudentCode = r.StudentCode.Trim(); x.FullName = r.FullName.Trim(); x.Email = r.Email.Trim(); x.ClassName = r.ClassName.Trim(); x.IsActive = r.IsActive; await db.SaveChangesAsync(ct); return Ok(Map(x)); }
    private static StudentDto Map(Student x) => new(x.Id, x.StudentCode, x.FullName, x.Email, x.ClassName, x.IsActive);
}
public record UpsertStudentRequest(string StudentCode, string FullName, string Email, string ClassName, bool IsActive = true);
public record StudentDto(Guid Id, string StudentCode, string FullName, string Email, string ClassName, bool IsActive);
