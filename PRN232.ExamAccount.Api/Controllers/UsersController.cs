using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Api.Security;
using PRN232.ExamAccount.Domain.Entities;
using PRN232.ExamAccount.Domain.Enums;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Api.Controllers;

[ApiController, Route("api/users"), Authorize(Roles = nameof(UserRole.Admin))]
public class UsersController(ExamAccountDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<UserDto>> GetAll(UserRole? role, CancellationToken ct)
    {
        var query = db.Users.AsNoTracking().AsQueryable();
        if (role.HasValue) query = query.Where(x => x.Role == role);
        return await query.OrderBy(x => x.FullName).Select(x => new UserDto(x.Id, x.UserName, x.FullName, x.Email, x.Role, x.IsActive)).ToListAsync(ct);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(UpsertUserRequest request, CancellationToken ct)
    {
        if (await db.Users.AnyAsync(x => x.UserName == request.UserName.Trim(), ct)) return Conflict("UserName đã tồn tại.");
        var user = new UserAccount { Id = Guid.NewGuid(), UserName = request.UserName.Trim(), PasswordHash = PasswordService.Hash(request.Password), FullName = request.FullName.Trim(), Email = request.Email.Trim(), Role = request.Role, IsActive = request.IsActive };
        db.Users.Add(user); await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetAll), new { id = user.Id }, Map(user));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserDto>> Update(Guid id, UpsertUserRequest request, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([id], ct); if (user is null) return NotFound();
        if (await db.Users.AnyAsync(x => x.Id != id && x.UserName == request.UserName.Trim(), ct)) return Conflict("UserName đã tồn tại.");
        user.UserName = request.UserName.Trim(); user.FullName = request.FullName.Trim(); user.Email = request.Email.Trim(); user.Role = request.Role; user.IsActive = request.IsActive;
        if (!string.IsNullOrWhiteSpace(request.Password)) user.PasswordHash = PasswordService.Hash(request.Password);
        await db.SaveChangesAsync(ct); return Ok(Map(user));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        if (id == User.CurrentUserId()) return BadRequest("Không thể xoá tài khoản đang đăng nhập.");
        var user = await db.Users.Include(x => x.AssignedBatches).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (user is null) return NotFound();
        if (user.AssignedBatches.Count > 0) return BadRequest("Tài khoản đã có lịch sử phân công; hãy khoá thay vì xoá.");
        db.Users.Remove(user); await db.SaveChangesAsync(ct); return NoContent();
    }
    private static UserDto Map(UserAccount x) => new(x.Id, x.UserName, x.FullName, x.Email, x.Role, x.IsActive);
}

public record UpsertUserRequest(string UserName, string Password, string FullName, string Email, UserRole Role, bool IsActive = true);
public record UserDto(Guid Id, string UserName, string FullName, string Email, UserRole Role, bool IsActive);
