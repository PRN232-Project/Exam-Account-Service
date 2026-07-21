using PRN232.ExamAccount.Domain.Enums;

namespace PRN232.ExamAccount.Domain.Entities;

public class UserAccount
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<GradingBatch> AssignedBatches { get; set; } = new List<GradingBatch>();
    public ICollection<NotificationRecord> Notifications { get; set; } = new List<NotificationRecord>();
}
