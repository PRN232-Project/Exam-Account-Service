using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Api.Security;
using PRN232.ExamAccount.Domain.Entities;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Api.Controllers;

[ApiController, Route("api/auth")]
public class AuthController(ExamAccountDbContext db, JwtTokenService tokens, IConfiguration configuration) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var user = await db.Users.SingleOrDefaultAsync(x => x.UserName == request.UserName && x.IsActive, ct);
        if (user is null || !PasswordService.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu." });
        return Ok(await IssueTokens(user, ct));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request, CancellationToken ct)
    {
        var hash = JwtTokenService.HashRefreshToken(request.RefreshToken);
        var stored = await db.RefreshTokens.Include(x => x.UserAccount)
            .SingleOrDefaultAsync(x => x.TokenHash == hash, ct);
        if (stored?.UserAccount is null || stored.RevokedAtUtc is not null || stored.ExpiresAtUtc <= DateTime.UtcNow || !stored.UserAccount.IsActive)
            return Unauthorized(new { message = "Refresh token không hợp lệ hoặc đã hết hạn." });
        stored.RevokedAtUtc = DateTime.UtcNow;
        return Ok(await IssueTokens(stored.UserAccount, ct));
    }

    private async Task<AuthResponse> IssueTokens(UserAccount user, CancellationToken ct)
    {
        var (accessToken, expiresAtUtc) = tokens.CreateAccessToken(user);
        var refreshToken = tokens.CreateRefreshToken();
        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(), UserAccountId = user.Id,
            TokenHash = JwtTokenService.HashRefreshToken(refreshToken),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(configuration.GetValue("Jwt:RefreshTokenDays", 7))
        });
        await db.SaveChangesAsync(ct);
        return new AuthResponse(accessToken, refreshToken, expiresAtUtc,
            new CurrentUserDto(user.Id, user.UserName, user.FullName, user.Email, user.Role.ToString()));
    }
}

public record LoginRequest(string UserName, string Password);
public record RefreshRequest(string RefreshToken);
public record CurrentUserDto(Guid Id, string UserName, string FullName, string Email, string Role);
public record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc, CurrentUserDto User);
