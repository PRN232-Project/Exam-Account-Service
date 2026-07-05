namespace PRN232.ExamAccount.Application.Dashboard;

public class ExamDashboardDto
{
    public Guid ExamId { get; set; }
    public string ExamCode { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    public decimal MaxScore { get; set; }
    public int CandidateCount { get; set; }
    public IReadOnlyList<CandidateDashboardDto> Candidates { get; set; } = [];
}

public class CandidateDashboardDto
{
    public Guid SubmissionId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal? TotalScore { get; set; }
    public DateTime SubmittedAtUtc { get; set; }
    public DateTime? GradedAtUtc { get; set; }
    public IReadOnlyList<SectionScoreDto> SectionResults { get; set; } = [];
}

public class SectionScoreDto
{
    public string SectionName { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Feedback { get; set; } = string.Empty;
}
