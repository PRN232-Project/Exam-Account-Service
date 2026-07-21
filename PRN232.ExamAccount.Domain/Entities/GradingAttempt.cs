namespace PRN232.ExamAccount.Domain.Entities;

public class GradingAttempt
{
    public Guid Id { get; set; }
    public Guid GradingItemId { get; set; }
    public int AttemptNumber { get; set; }
    public string ClientRequestId { get; set; } = string.Empty;
    public decimal TotalScore { get; set; }
    public string RawJsonReport { get; set; } = string.Empty;
    public bool HasTechnicalError { get; set; }
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string RubricVersion { get; set; } = string.Empty;
    public DateTime CompletedAtUtc { get; set; }
    public GradingItem? GradingItem { get; set; }
}
