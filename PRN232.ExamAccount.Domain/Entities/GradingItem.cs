using PRN232.ExamAccount.Domain.Enums;

namespace PRN232.ExamAccount.Domain.Entities;

public class GradingItem
{
    public Guid Id { get; set; }
    public Guid GradingBatchId { get; set; }
    public Guid ExamCandidateId { get; set; }
    public GradingItemStatus Status { get; set; } = GradingItemStatus.Assigned;
    public decimal? LatestScore { get; set; }
    public int LatestAttemptNumber { get; set; }
    public string LastErrorCode { get; set; } = string.Empty;
    public string LastErrorMessage { get; set; } = string.Empty;
    public GradingBatch? GradingBatch { get; set; }
    public ExamCandidate? ExamCandidate { get; set; }
    public ICollection<GradingAttempt> Attempts { get; set; } = new List<GradingAttempt>();
    public ICollection<ReviewRequest> ReviewRequests { get; set; } = new List<ReviewRequest>();
}
