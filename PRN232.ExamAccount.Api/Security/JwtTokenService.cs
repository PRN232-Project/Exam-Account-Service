using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PRN232.ExamAccount.Domain.Entities;

namespace PRN232.ExamAccount.Api.Security;

public class JwtTokenService(IConfiguration configuration)
{
    public (string Token, DateTime ExpiresAtUtc) CreateAccessToken(UserAccount user)
    {
        var expires = DateTime.UtcNow.AddMinutes(configuration.GetValue("Jwt:AccessTokenMinutes", 30));
        var credentials = new SigningCredentials(GetKey(), SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("full_name", user.FullName)
        };
        var jwt = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"], audience: configuration["Jwt:Audience"],
            claims: claims, expires: expires, signingCredentials: credentials);
        return (new JwtSecurityTokenHandler().WriteToken(jwt), expires);
    }

    public string CreateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
    public static string HashRefreshToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    private SymmetricSecurityKey GetKey() => new(Encoding.UTF8.GetBytes(
        configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.")));
}
