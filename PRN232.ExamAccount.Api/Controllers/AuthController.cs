using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ExamAccountDbContext _dbContext;

    public AuthController(ExamAccountDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Students
            .AsNoTracking()
            .Include(x => x.ManagedRooms)
            .FirstOrDefaultAsync(x => x.UserName == request.UserName && x.IsActive, cancellationToken);

        if (user is null || user.PasswordHash != HashPassword(request.Password))
        {
            return Unauthorized(new { message = "Sai tai khoan hoac mat khau." });
        }

        return Ok(new LoginResponse
        {
            UserId = user.Id,
            UserName = user.UserName,
            FullName = user.FullName,
            Email = user.Email,
            StudentCode = user.StudentCode,
            Role = user.Role.ToString(),
            ManagedRoomIds = user.ManagedRooms.Select(x => x.Id).ToList()
        });
    }

    internal static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
    }
}

public record LoginRequest(string UserName, string Password);

public class LoginResponse
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public IReadOnlyList<Guid> ManagedRoomIds { get; set; } = [];
}
