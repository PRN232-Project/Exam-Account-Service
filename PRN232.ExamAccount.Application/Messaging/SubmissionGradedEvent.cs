namespace PRN232.ExamAccount.Application.Messaging;

public class SubmissionGradedEvent
{
    public Guid SubmissionId { get; set; }
    public decimal TotalScore { get; set; }
    public string RawJsonReport { get; set; } = string.Empty;
    public DateTime CompletedAtUtc { get; set; } = DateTime.UtcNow;
}
