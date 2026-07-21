using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Api.Security;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Api.Controllers;

[ApiController, Route("api/notifications"), Authorize]
public class NotificationsController(ExamAccountDbContext db) : ControllerBase
{
    [HttpGet]
    public Task<List<NotificationDto>> GetMine(CancellationToken ct) { var userId = User.CurrentUserId(); return db.Notifications.AsNoTracking().Where(x => x.RecipientUserId == userId).OrderByDescending(x => x.CreatedAtUtc).Select(x => new NotificationDto(x.Id, x.GradingBatchId, x.GradingItemId, x.Type, x.Title, x.Message, x.IsRead, x.CreatedAtUtc)).ToListAsync(ct); }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> Read(Guid id, CancellationToken ct) { var userId = User.CurrentUserId(); var x = await db.Notifications.SingleOrDefaultAsync(n => n.Id == id && n.RecipientUserId == userId, ct); if (x is null) return NotFound(); x.IsRead = true; await db.SaveChangesAsync(ct); return NoContent(); }
}
public record NotificationDto(Guid Id, Guid? GradingBatchId, Guid? GradingItemId, string Type, string Title, string Message, bool IsRead, DateTime CreatedAtUtc);
