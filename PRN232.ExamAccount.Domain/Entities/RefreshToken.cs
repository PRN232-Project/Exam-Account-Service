namespace PRN232.ExamAccount.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserAccountId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public UserAccount? UserAccount { get; set; }
}
