namespace PRN232.ExamAccount.Domain.Entities;

public class ExamRoom
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid LecturerId { get; set; }

    public StudentAccount? Lecturer { get; set; }
    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
