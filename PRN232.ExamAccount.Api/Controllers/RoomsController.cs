using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Domain.Entities;
using PRN232.ExamAccount.Domain.Enums;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Api.Controllers;

[ApiController, Route("api/rooms"), Authorize(Roles = nameof(UserRole.ExamOfficer))]
public class RoomsController(ExamAccountDbContext db) : ControllerBase
{
    [HttpGet] public Task<List<RoomDto>> GetAll(CancellationToken ct) => db.ExamRooms.AsNoTracking().OrderBy(x => x.Code).Select(x => new RoomDto(x.Id, x.Code, x.Name, x.Location, x.IsActive)).ToListAsync(ct);
    [HttpPost] public async Task<ActionResult<RoomDto>> Create(UpsertRoomRequest r, CancellationToken ct) { if (await db.ExamRooms.AnyAsync(x => x.Code == r.Code.Trim(), ct)) return Conflict("Mã phòng đã tồn tại."); var x = new ExamRoom { Id = Guid.NewGuid(), Code = r.Code.Trim(), Name = r.Name.Trim(), Location = r.Location.Trim(), IsActive = r.IsActive }; db.Add(x); await db.SaveChangesAsync(ct); return Created("api/rooms/" + x.Id, Map(x)); }
    [HttpPut("{id:guid}")] public async Task<ActionResult<RoomDto>> Update(Guid id, UpsertRoomRequest r, CancellationToken ct) { var x = await db.ExamRooms.FindAsync([id], ct); if (x is null) return NotFound(); if (await db.ExamRooms.AnyAsync(y => y.Id != id && y.Code == r.Code.Trim(), ct)) return Conflict("Mã phòng đã tồn tại."); x.Code = r.Code.Trim(); x.Name = r.Name.Trim(); x.Location = r.Location.Trim(); x.IsActive = r.IsActive; await db.SaveChangesAsync(ct); return Ok(Map(x)); }
    [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { var x = await db.ExamRooms.Include(y => y.Sessions).FirstOrDefaultAsync(y => y.Id == id, ct); if (x is null) return NotFound(); if (x.Sessions.Count > 0) return BadRequest("Phòng đã có ca thi; hãy khoá thay vì xoá."); db.Remove(x); await db.SaveChangesAsync(ct); return NoContent(); }
    private static RoomDto Map(ExamRoom x) => new(x.Id, x.Code, x.Name, x.Location, x.IsActive);
}
public record UpsertRoomRequest(string Code, string Name, string Location, bool IsActive = true);
public record RoomDto(Guid Id, string Code, string Name, string Location, bool IsActive);
