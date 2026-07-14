namespace PRN232.ExamAccount.Domain.Entities;

public class NotificationRecord
{
    public Guid Id { get; set; }
    public Guid RecipientUserId { get; set; }
    public Guid? SubmissionId { get; set; }
    public Guid? ExamId { get; set; }
    public Guid? RoomId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public StudentAccount? RecipientUser { get; set; }
}
