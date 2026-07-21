namespace PRN232.ExamAccount.Domain.Entities;

public class ExamSectionDefinition
{
    public Guid Id { get; set; }
    public Guid ExamPaperId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string TestFilter { get; set; } = string.Empty;
    public ExamPaper? ExamPaper { get; set; }
}
