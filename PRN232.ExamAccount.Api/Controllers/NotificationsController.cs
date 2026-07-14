using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Api.Security;
using PRN232.ExamAccount.Domain.Enums;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly ExamAccountDbContext _dbContext;

    public NotificationsController(ExamAccountDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetNotificationsAsync([FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin, UserRole.Lecturer, UserRole.Student);
        if (auth.Error is not null) return auth.Error;
        if (auth.User!.Role != UserRole.Admin && auth.User.Id != userId)
        {
            return Forbid();
        }

        var data = await _dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.RecipientUserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new NotificationDto
            {
                Id = x.Id,
                Type = x.Type,
                Title = x.Title,
                Message = x.Message,
                IsRead = x.IsRead,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(data);
    }

    [HttpGet("{notificationId:guid}")]
    public async Task<ActionResult<NotificationDto>> GetNotificationByIdAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin, UserRole.Lecturer, UserRole.Student);
        if (auth.Error is not null) return auth.Error;

        var notification = await _dbContext.Notifications
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == notificationId, cancellationToken);

        if (notification is null)
        {
            return NotFound();
        }

        if (auth.User!.Role != UserRole.Admin && notification.RecipientUserId != auth.User.Id)
        {
            return Forbid();
        }

        return Ok(MapNotification(notification));
    }

    [HttpPost("{notificationId:guid}/read")]
    public async Task<ActionResult<NotificationDto>> MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin, UserRole.Lecturer, UserRole.Student);
        if (auth.Error is not null) return auth.Error;

        var notification = await _dbContext.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId, cancellationToken);
        if (notification is null)
        {
            return NotFound();
        }

        if (auth.User!.Role != UserRole.Admin && notification.RecipientUserId != auth.User.Id)
        {
            return Forbid();
        }

        notification.IsRead = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(MapNotification(notification));
    }

    [HttpDelete("{notificationId:guid}")]
    public async Task<IActionResult> DeleteNotificationAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        var auth = await this.RequireRolesAsync(_dbContext, UserRole.Admin, UserRole.Lecturer, UserRole.Student);
        if (auth.Error is not null) return auth.Error;

        var notification = await _dbContext.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId, cancellationToken);
        if (notification is null)
        {
            return NotFound();
        }

        if (auth.User!.Role != UserRole.Admin && notification.RecipientUserId != auth.User.Id)
        {
            return Forbid();
        }

        _dbContext.Notifications.Remove(notification);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static NotificationDto MapNotification(Domain.Entities.NotificationRecord notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            IsRead = notification.IsRead,
            CreatedAtUtc = notification.CreatedAtUtc
        };
    }
}

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
