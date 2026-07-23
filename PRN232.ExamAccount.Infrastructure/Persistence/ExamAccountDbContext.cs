using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Domain.Entities;

namespace PRN232.ExamAccount.Infrastructure.Persistence;

public class ExamAccountDbContext(DbContextOptions<ExamAccountDbContext> options) : DbContext(options)
{
    public DbSet<UserAccount> Users => Set<UserAccount>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<ExamRoom> ExamRooms => Set<ExamRoom>();
    public DbSet<ExamPaper> ExamPapers => Set<ExamPaper>();
    public DbSet<ExamSectionDefinition> ExamSections => Set<ExamSectionDefinition>();
    public DbSet<ExamSession> ExamSessions => Set<ExamSession>();
    public DbSet<ExamCandidate> ExamCandidates => Set<ExamCandidate>();
    public DbSet<GradingBatch> GradingBatches => Set<GradingBatch>();
    public DbSet<GradingItem> GradingItems => Set<GradingItem>();
    public DbSet<GradingAttempt> GradingAttempts => Set<GradingAttempt>();
    public DbSet<ReviewRequest> ReviewRequests => Set<ReviewRequest>();
    public DbSet<NotificationRecord> Notifications => Set<NotificationRecord>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<BatchExecutionToken> BatchExecutionTokens => Set<BatchExecutionToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("exam");

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.UserName).IsUnique();
            entity.Property(x => x.UserName).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256);
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(32).IsRequired();
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.StudentCode).IsUnique();
            entity.Property(x => x.StudentCode).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256);
            entity.Property(x => x.ClassName).HasMaxLength(128);
        });

        modelBuilder.Entity<ExamRoom>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Location).HasMaxLength(256);
        });

        modelBuilder.Entity<ExamPaper>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.RubricVersion).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SolutionPattern).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PlagiarismKeywords).HasColumnType("text[]");
        });

        modelBuilder.Entity<ExamSectionDefinition>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.ExamPaperId, x.Name }).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.TestFilter).HasMaxLength(256).IsRequired();
            entity.Property(x => x.TestCasesJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.ApiProjectPath).HasMaxLength(512);
            entity.HasOne(x => x.ExamPaper).WithMany(x => x.Sections)
                .HasForeignKey(x => x.ExamPaperId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExamSession>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.HasOne(x => x.Room).WithMany(x => x.Sessions)
                .HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ExamPaper).WithMany(x => x.Sessions)
                .HasForeignKey(x => x.ExamPaperId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ExamCandidate>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.ExamSessionId, x.StudentId }).IsUnique();
            entity.Property(x => x.PaperCode).HasMaxLength(64).IsRequired();
            entity.HasOne(x => x.ExamSession).WithMany(x => x.Candidates)
                .HasForeignKey(x => x.ExamSessionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Student).WithMany(x => x.ExamCandidates)
                .HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GradingBatch>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.HasOne(x => x.ExamSession).WithMany(x => x.GradingBatches)
                .HasForeignKey(x => x.ExamSessionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Lecturer).WithMany(x => x.AssignedBatches)
                .HasForeignKey(x => x.LecturerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GradingItem>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.ExamCandidateId).IsUnique();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(64).IsRequired();
            entity.Property(x => x.LastErrorCode).HasMaxLength(64);
            entity.Property(x => x.LastErrorMessage).HasMaxLength(2048);
            entity.Property(x => x.PlagiarismStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.PlagiarismReportJson).HasColumnType("jsonb");
            entity.Property(x => x.PlagiarismErrorMessage).HasMaxLength(2048);
            entity.HasOne(x => x.GradingBatch).WithMany(x => x.Items)
                .HasForeignKey(x => x.GradingBatchId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ExamCandidate).WithOne(x => x.GradingItem)
                .HasForeignKey<GradingItem>(x => x.ExamCandidateId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GradingAttempt>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.GradingItemId, x.AttemptNumber }).IsUnique();
            entity.HasIndex(x => x.ClientRequestId).IsUnique();
            entity.Property(x => x.ClientRequestId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ErrorCode).HasMaxLength(64);
            entity.Property(x => x.ErrorMessage).HasMaxLength(2048);
            entity.Property(x => x.RubricVersion).HasMaxLength(64).IsRequired();
            entity.HasOne(x => x.GradingItem).WithMany(x => x.Attempts)
                .HasForeignKey(x => x.GradingItemId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReviewRequest>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Reason).HasMaxLength(2048).IsRequired();
            entity.HasOne(x => x.GradingItem).WithMany(x => x.ReviewRequests)
                .HasForeignKey(x => x.GradingItemId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NotificationRecord>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Type).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(2048).IsRequired();
            entity.HasOne(x => x.RecipientUser).WithMany(x => x.Notifications)
                .HasForeignKey(x => x.RecipientUserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Action).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ResourceType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.DetailsJson).HasColumnType("jsonb");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            entity.HasOne(x => x.UserAccount).WithMany()
                .HasForeignKey(x => x.UserAccountId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BatchExecutionToken>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            entity.HasOne(x => x.GradingBatch).WithMany(x => x.ExecutionTokens)
                .HasForeignKey(x => x.GradingBatchId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
