namespace PRN232.ExamAccount.Domain.Entities;

public class StudentAccount
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Enums.UserRole Role { get; set; } = Enums.UserRole.Student;
    public bool IsActive { get; set; } = true;

    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    public ICollection<ExamRoom> ManagedRooms { get; set; } = new List<ExamRoom>();
    public ICollection<NotificationRecord> Notifications { get; set; } = new List<NotificationRecord>();
}
