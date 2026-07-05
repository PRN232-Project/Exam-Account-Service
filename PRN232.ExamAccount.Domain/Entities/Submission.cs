using PRN232.ExamAccount.Domain.Enums;

namespace PRN232.ExamAccount.Domain.Entities;

public class Submission
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public Guid StudentAccountId { get; set; }
    public string WorkspacePath { get; set; } = string.Empty;
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;
    public decimal? TotalScore { get; set; }
    public string RawJsonReport { get; set; } = string.Empty;
    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? GradedAtUtc { get; set; }

    public Exam? Exam { get; set; }
    public StudentAccount? StudentAccount { get; set; }
    public ICollection<SubmissionSectionResult> SectionResults { get; set; } = new List<SubmissionSectionResult>();
}
