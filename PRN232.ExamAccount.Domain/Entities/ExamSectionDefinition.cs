namespace PRN232.ExamAccount.Domain.Entities;

public class ExamSectionDefinition
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string TestFilter { get; set; } = string.Empty;

    public Exam? Exam { get; set; }
}
