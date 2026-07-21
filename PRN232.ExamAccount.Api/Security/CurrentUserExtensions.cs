using System.Security.Claims;

namespace PRN232.ExamAccount.Api.Security;

public static class CurrentUserExtensions
{
    public static Guid CurrentUserId(this ClaimsPrincipal user) =>
        Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id : throw new UnauthorizedAccessException("Token thiếu user id.");
}
