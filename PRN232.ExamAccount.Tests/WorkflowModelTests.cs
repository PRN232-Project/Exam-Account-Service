using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Api.Security;
using PRN232.ExamAccount.Domain.Entities;
using PRN232.ExamAccount.Domain.Enums;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Tests;

public class WorkflowModelTests
{
    [Fact]
    public void PasswordHash_UsesSalt_AndCanBeVerified()
    {
        var first = PasswordService.Hash("123456");
        var second = PasswordService.Hash("123456");
        Assert.NotEqual(first, second);
        Assert.True(PasswordService.Verify("123456", first));
        Assert.False(PasswordService.Verify("wrong", first));
    }

    [Fact]
    public async Task Student_IsMasterData_NotAUserAccount()
    {
        await using var db = CreateDb();
        db.Users.Add(new UserAccount { Id = Guid.NewGuid(), UserName = "officer", PasswordHash = "hash", FullName = "Officer", Role = UserRole.ExamOfficer });
        db.Students.Add(new Student { Id = Guid.NewGuid(), StudentCode = "SE180001", FullName = "Student One" });
        await db.SaveChangesAsync();
        Assert.Single(await db.Users.ToListAsync());
        Assert.Single(await db.Students.ToListAsync());
        Assert.DoesNotContain(await db.Users.ToListAsync(), x => x.UserName == "SE180001");
    }

    [Fact]
    public void WorkflowStatuses_ContainReviewLoop()
    {
        Assert.True(Enum.IsDefined(GradingBatchStatus.NeedsCorrection));
        Assert.True(Enum.IsDefined(GradingItemStatus.ReturnedForCorrection));
        Assert.True(Enum.IsDefined(GradingItemStatus.TechnicalError));
    }

    private static ExamAccountDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ExamAccountDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new ExamAccountDbContext(options);
    }
}
