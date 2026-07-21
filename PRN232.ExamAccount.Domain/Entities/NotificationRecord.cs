namespace PRN232.ExamAccount.Domain.Entities;

public class NotificationRecord
{
    public Guid Id { get; set; }
    public Guid RecipientUserId { get; set; }
    public Guid? GradingBatchId { get; set; }
    public Guid? GradingItemId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public UserAccount? RecipientUser { get; set; }
}
