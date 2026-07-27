using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Grpc.Core;
using Microsoft.IdentityModel.Tokens;
using PRN232.Common.Grpc;

namespace PRN232.ExamAccount.Api.Services;

public class AuthGrpcEndpoint(IConfiguration configuration, ILogger<AuthGrpcEndpoint> logger)
    : AuthGrpcService.AuthGrpcServiceBase
{
    public override Task<VerifyTokenResponse> VerifyToken(VerifyTokenRequest request, ServerCallContext context)
    {
        var rawToken = request.Token;
        if (string.IsNullOrEmpty(rawToken))
        {
            return Task.FromResult(new VerifyTokenResponse { IsValid = false });
        }

        if (rawToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            rawToken = rawToken["Bearer ".Length..].Trim();
        }

        try
        {
            var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.FromSeconds(30)
            };

            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(rawToken, validationParameters, out _);

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                         ?? string.Empty;
            var username = principal.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
            var role = principal.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            logger.LogInformation("gRPC Token Verification Success. User: {Username}, Role: {Role}", username, role);

            return Task.FromResult(new VerifyTokenResponse
            {
                IsValid = true,
                UserId = userId,
                Username = username,
                Role = role
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning("gRPC Token Verification Failed: {Message}", ex.Message);
            return Task.FromResult(new VerifyTokenResponse { IsValid = false });
        }
    }
}
