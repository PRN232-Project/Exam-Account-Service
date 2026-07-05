namespace PRN232.ExamAccount.Domain.Entities;

public class SubmissionSectionResult
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Feedback { get; set; } = string.Empty;

    public Submission? Submission { get; set; }
}
