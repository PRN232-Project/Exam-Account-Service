namespace PRN232.ExamAccount.Domain.Entities;

public class ExamRoom
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<ExamSession> Sessions { get; set; } = new List<ExamSession>();
}
