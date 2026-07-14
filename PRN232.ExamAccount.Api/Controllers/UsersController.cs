using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Api.Security;
using PRN232.ExamAccount.Domain.Entities;
using PRN232.ExamAccount.Domain.Enums;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly ExamAccountDbContext _dbContext;

    public UsersController(ExamAccountDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetUsersAsync([FromQuery] UserRole? role, CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin);
        if (auth.Error is not null) return auth.Error;

        var query = _dbContext.Students.AsNoTracking().AsQueryable();
        if (role.HasValue)
        {
            query = query.Where(x => x.Role == role.Value);
        }

        var users = await query.OrderBy(x => x.FullName).Select(x => new UserDto
        {
            Id = x.Id,
            UserName = x.UserName,
            StudentCode = x.StudentCode,
            FullName = x.FullName,
            Email = x.Email,
            Role = x.Role.ToString(),
            IsActive = x.IsActive
        }).ToListAsync(cancellationToken);

        return Ok(users);
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<UserDto>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin);
        if (auth.Error is not null) return auth.Error;

        var user = await _dbContext.Students.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        return user is null ? NotFound() : Ok(MapUser(user));
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUserAsync([FromBody] UpsertUserRequest request, CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin);
        if (auth.Error is not null) return auth.Error;

        var exists = await _dbContext.Students.AnyAsync(x => x.UserName == request.UserName, cancellationToken);
        if (exists)
        {
            return BadRequest("UserName da ton tai.");
        }

        var user = new StudentAccount
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName.Trim(),
            PasswordHash = AuthController.HashPassword(request.Password),
            StudentCode = request.StudentCode.Trim(),
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            Role = request.Role,
            IsActive = request.IsActive
        };

        _dbContext.Students.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(MapUser(user));
    }

    [HttpPut("{userId:guid}")]
    public async Task<ActionResult<UserDto>> UpdateUserAsync(Guid userId, [FromBody] UpsertUserRequest request, CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin);
        if (auth.Error is not null) return auth.Error;

        var duplicateUserName = await _dbContext.Students.AnyAsync(
            x => x.Id != userId && x.UserName == request.UserName,
            cancellationToken);
        if (duplicateUserName)
        {
            return BadRequest("UserName da ton tai.");
        }

        var user = await _dbContext.Students.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        user.UserName = request.UserName.Trim();
        user.StudentCode = request.StudentCode.Trim();
        user.FullName = request.FullName.Trim();
        user.Email = request.Email.Trim();
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = AuthController.HashPassword(request.Password);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(MapUser(user));
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> DeleteUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin);
        if (auth.Error is not null) return auth.Error;

        var user = await _dbContext.Students
            .Include(x => x.ManagedRooms)
            .Include(x => x.Submissions)
            .Include(x => x.Notifications)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        if (user.ManagedRooms.Count > 0)
        {
            return BadRequest("Khong the xoa lecturer dang duoc gan cho room.");
        }

        if (user.Submissions.Count > 0)
        {
            return BadRequest("Khong the xoa user da co submission.");
        }

        if (user.Notifications.Count > 0)
        {
            _dbContext.Notifications.RemoveRange(user.Notifications);
        }

        _dbContext.Students.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static UserDto MapUser(StudentAccount user)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            StudentCode = user.StudentCode,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            IsActive = user.IsActive
        };
    }
}

public class UpsertUserRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Student;
    public bool IsActive { get; set; } = true;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
