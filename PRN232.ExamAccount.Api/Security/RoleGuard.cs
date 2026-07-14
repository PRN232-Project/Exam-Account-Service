using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Domain.Entities;
using PRN232.ExamAccount.Domain.Enums;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Api.Security;

public static class RoleGuard
{
    public static async Task<(StudentAccount? User, ActionResult? Error)> RequireRolesAsync(
        this ControllerBase controller,
        ExamAccountDbContext dbContext,
        params UserRole[] allowedRoles)
    {
        if (!controller.Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) ||
            !Guid.TryParse(userIdHeader.FirstOrDefault(), out var userId))
        {
            return (null, controller.Unauthorized("Missing X-User-Id header."));
        }

        if (!controller.Request.Headers.TryGetValue("X-User-Role", out var roleHeader) ||
            !Enum.TryParse<UserRole>(roleHeader.FirstOrDefault(), true, out var role))
        {
            return (null, controller.Unauthorized("Missing X-User-Role header."));
        }

        var user = await dbContext.Students.FirstOrDefaultAsync(x => x.Id == userId && x.IsActive);
        if (user is null || user.Role != role)
        {
            return (null, controller.Forbid());
        }

        if (allowedRoles.Length > 0 && !allowedRoles.Contains(user.Role))
        {
            return (null, controller.Forbid());
        }

        return (user, null);
    }
}
