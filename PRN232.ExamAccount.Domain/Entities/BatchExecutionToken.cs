namespace PRN232.ExamAccount.Domain.Entities;

public class BatchExecutionToken
{
    public Guid Id { get; set; }
    public Guid GradingBatchId { get; set; }
    public Guid IssuedByUserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public GradingBatch? GradingBatch { get; set; }
}
