using PRN232.ExamAccount.Domain.Enums;

namespace PRN232.ExamAccount.Domain.Entities;

public class ExamSession
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public Guid RoomId { get; set; }
    public Guid ExamPaperId { get; set; }
    public DateTime ScheduledAtUtc { get; set; }
    public ExamSessionStatus Status { get; set; } = ExamSessionStatus.Draft;
    public ExamRoom? Room { get; set; }
    public ExamPaper? ExamPaper { get; set; }
    public ICollection<ExamCandidate> Candidates { get; set; } = new List<ExamCandidate>();
    public ICollection<GradingBatch> GradingBatches { get; set; } = new List<GradingBatch>();
}
