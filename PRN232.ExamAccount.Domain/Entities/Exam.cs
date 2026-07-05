namespace PRN232.ExamAccount.Domain.Entities;

public class Exam
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal MaxScore { get; set; } = 10m;
    public string SolutionPattern { get; set; } = string.Empty;
    public bool RequireAppSettings { get; set; } = true;
    public bool ForbidHardcodedConnectionString { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 15;
    public string[] PlagiarismKeywords { get; set; } = [];
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<ExamSectionDefinition> Sections { get; set; } = new List<ExamSectionDefinition>();
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
