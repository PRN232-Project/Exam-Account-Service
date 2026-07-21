namespace PRN232.ExamAccount.Domain.Entities;

public class ExamCandidate
{
    public Guid Id { get; set; }
    public Guid ExamSessionId { get; set; }
    public Guid StudentId { get; set; }
    public string PaperCode { get; set; } = string.Empty;
    public bool IsAbsent { get; set; }
    public ExamSession? ExamSession { get; set; }
    public Student? Student { get; set; }
    public GradingItem? GradingItem { get; set; }
}
