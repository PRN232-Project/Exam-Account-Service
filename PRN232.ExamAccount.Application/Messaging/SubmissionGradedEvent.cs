namespace PRN232.ExamAccount.Application.Messaging;

public class SubmissionGradedEvent
{
    public Guid SubmissionId { get; set; }
    public Guid ExamId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public decimal TotalScore { get; set; }
    public string RawJsonReport { get; set; } = string.Empty;
    public bool HasErrors { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime CompletedAtUtc { get; set; } = DateTime.UtcNow;
}
