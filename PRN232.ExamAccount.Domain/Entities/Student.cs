namespace PRN232.ExamAccount.Domain.Entities;

public class Student
{
    public Guid Id { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<ExamCandidate> ExamCandidates { get; set; } = new List<ExamCandidate>();
}
