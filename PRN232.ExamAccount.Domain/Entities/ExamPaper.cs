namespace PRN232.ExamAccount.Domain.Entities;

public class ExamPaper
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string RubricVersion { get; set; } = "1";
    public decimal MaxScore { get; set; } = 10m;
    public string SolutionPattern { get; set; } = "*.sln";
    public bool RequireAppSettings { get; set; } = true;
    public bool ForbidHardcodedConnectionString { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 15;
    public string[] PlagiarismKeywords { get; set; } = [];
    public bool IsActive { get; set; } = true;
    public ICollection<ExamSectionDefinition> Sections { get; set; } = new List<ExamSectionDefinition>();
    public ICollection<ExamSession> Sessions { get; set; } = new List<ExamSession>();
}
