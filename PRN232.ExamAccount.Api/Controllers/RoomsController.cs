using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Api.Security;
using PRN232.ExamAccount.Domain.Entities;
using PRN232.ExamAccount.Domain.Enums;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Api.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController : ControllerBase
{
    private readonly ExamAccountDbContext _dbContext;

    public RoomsController(ExamAccountDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoomDto>>> GetRoomsAsync(CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin, UserRole.Lecturer);
        if (auth.Error is not null) return auth.Error;

        var query = _dbContext.ExamRooms.AsNoTracking().Include(x => x.Lecturer).AsQueryable();
        if (auth.User!.Role == UserRole.Lecturer)
        {
            query = query.Where(x => x.LecturerId == auth.User.Id);
        }

        var rooms = await query.Select(x => new RoomDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            LecturerId = x.LecturerId,
            LecturerName = x.Lecturer != null ? x.Lecturer.FullName : string.Empty
        }).ToListAsync(cancellationToken);

        return Ok(rooms);
    }

    [HttpGet("{roomId:guid}")]
    public async Task<ActionResult<RoomDto>> GetRoomByIdAsync(Guid roomId, CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin, UserRole.Lecturer);
        if (auth.Error is not null) return auth.Error;

        var room = await _dbContext.ExamRooms
            .AsNoTracking()
            .Include(x => x.Lecturer)
            .FirstOrDefaultAsync(x => x.Id == roomId, cancellationToken);

        if (room is null)
        {
            return NotFound();
        }

        if (auth.User!.Role == UserRole.Lecturer && room.LecturerId != auth.User.Id)
        {
            return Forbid();
        }

        return Ok(MapRoom(room, room.Lecturer));
    }

    [HttpPost]
    public async Task<ActionResult<RoomDto>> CreateRoomAsync([FromBody] UpsertRoomRequest request, CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin);
        if (auth.Error is not null) return auth.Error;

        var lecturer = await _dbContext.Students.FirstOrDefaultAsync(x => x.Id == request.LecturerId, cancellationToken);
        if (lecturer is null || lecturer.Role != UserRole.Lecturer)
        {
            return BadRequest("LecturerId khong hop le.");
        }

        var room = new ExamRoom
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            LecturerId = request.LecturerId
        };

        _dbContext.ExamRooms.Add(room);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new RoomDto
        {
            Id = room.Id,
            Code = room.Code,
            Name = room.Name,
            LecturerId = lecturer.Id,
            LecturerName = lecturer.FullName
        });
    }

    [HttpPut("{roomId:guid}")]
    public async Task<ActionResult<RoomDto>> UpdateRoomAsync(Guid roomId, [FromBody] UpsertRoomRequest request, CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin);
        if (auth.Error is not null) return auth.Error;

        var room = await _dbContext.ExamRooms.FirstOrDefaultAsync(x => x.Id == roomId, cancellationToken);
        if (room is null)
        {
            return NotFound();
        }

        var lecturer = await _dbContext.Students.FirstOrDefaultAsync(x => x.Id == request.LecturerId, cancellationToken);
        if (lecturer is null || lecturer.Role != UserRole.Lecturer)
        {
            return BadRequest("LecturerId khong hop le.");
        }

        room.Code = request.Code.Trim();
        room.Name = request.Name.Trim();
        room.LecturerId = request.LecturerId;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(MapRoom(room, lecturer));
    }

    [HttpDelete("{roomId:guid}")]
    public async Task<IActionResult> DeleteRoomAsync(Guid roomId, CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin);
        if (auth.Error is not null) return auth.Error;

        var room = await _dbContext.ExamRooms
            .Include(x => x.Exams)
            .FirstOrDefaultAsync(x => x.Id == roomId, cancellationToken);

        if (room is null)
        {
            return NotFound();
        }

        if (room.Exams.Count > 0)
        {
            return BadRequest("Khong the xoa room dang co exam.");
        }

        _dbContext.ExamRooms.Remove(room);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static RoomDto MapRoom(ExamRoom room, StudentAccount? lecturer)
    {
        return new RoomDto
        {
            Id = room.Id,
            Code = room.Code,
            Name = room.Name,
            LecturerId = room.LecturerId,
            LecturerName = lecturer?.FullName ?? string.Empty
        };
    }
}

public class UpsertRoomRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid LecturerId { get; set; }
}

public class RoomDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid LecturerId { get; set; }
    public string LecturerName { get; set; } = string.Empty;
}
