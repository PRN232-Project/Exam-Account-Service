using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Domain.Entities;

namespace PRN232.ExamAccount.Infrastructure.Persistence;

public class ExamAccountDbContext : DbContext
{
    public ExamAccountDbContext(DbContextOptions<ExamAccountDbContext> options) : base(options)
    {
    }

    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<ExamSectionDefinition> ExamSections => Set<ExamSectionDefinition>();
    public DbSet<StudentAccount> Students => Set<StudentAccount>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<SubmissionSectionResult> SubmissionSectionResults => Set<SubmissionSectionResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("exam");

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.SolutionPattern).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PlagiarismKeywords).HasColumnType("text[]");
            entity.HasMany(x => x.Sections)
                .WithOne(x => x.Exam)
                .HasForeignKey(x => x.ExamId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Submissions)
                .WithOne(x => x.Exam)
                .HasForeignKey(x => x.ExamId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExamSectionDefinition>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.TestFilter).HasMaxLength(256).IsRequired();
        });

        modelBuilder.Entity<StudentAccount>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.StudentCode).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256);
        });

        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.WorkspacePath).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.RawJsonReport);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(64).IsRequired();
            entity.HasOne(x => x.StudentAccount)
                .WithMany(x => x.Submissions)
                .HasForeignKey(x => x.StudentAccountId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.SectionResults)
                .WithOne(x => x.Submission)
                .HasForeignKey(x => x.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SubmissionSectionResult>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SectionName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
        });
    }
}
