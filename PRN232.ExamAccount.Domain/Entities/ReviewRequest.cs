namespace PRN232.ExamAccount.Domain.Entities;

public class ReviewRequest
{
    public Guid Id { get; set; }
    public Guid GradingItemId { get; set; }
    public Guid RequestedByUserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool IsResolved { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAtUtc { get; set; }
    public GradingItem? GradingItem { get; set; }
}
