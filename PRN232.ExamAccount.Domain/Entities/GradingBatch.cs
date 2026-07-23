using PRN232.ExamAccount.Domain.Enums;

namespace PRN232.ExamAccount.Domain.Entities;

public class GradingBatch
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid ExamSessionId { get; set; }
    public Guid LecturerId { get; set; }
    public GradingBatchStatus Status { get; set; } = GradingBatchStatus.Assigned;
    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAtUtc { get; set; }
    public DateTime? AcceptedAtUtc { get; set; }
    public ExamSession? ExamSession { get; set; }
    public UserAccount? Lecturer { get; set; }
    public ICollection<GradingItem> Items { get; set; } = new List<GradingItem>();
    public ICollection<BatchExecutionToken> ExecutionTokens { get; set; } = new List<BatchExecutionToken>();
}
