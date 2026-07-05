namespace PRN232.ExamAccount.Application.Messaging;

public class GradingJobMessage
{
    public Guid SubmissionId { get; set; }
    public Guid ExamId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string WorkspacePath { get; set; } = string.Empty;
    public bool IsRegrade { get; set; }
    public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;
    public ExamConfigurationMessage ExamConfig { get; set; } = new();
}

public class ExamConfigurationMessage
{
    public Guid ExamId { get; set; }
    public string ExamCode { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    public decimal MaxScore { get; set; }
    public string SolutionPattern { get; set; } = string.Empty;
    public bool RequireAppSettings { get; set; }
    public bool ForbidHardcodedConnectionString { get; set; }
    public int TimeoutSeconds { get; set; }
    public string[] PlagiarismConfig { get; set; } = [];
    public IReadOnlyList<SectionConfigurationMessage> Sections { get; set; } = [];
}

public class SectionConfigurationMessage
{
    public string Name { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string TestFilter { get; set; } = string.Empty;
}
