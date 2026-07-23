using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PRN232.ExamAccount.Api.Controllers;
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

    [Fact]
    public async Task AddCandidates_InsertsCandidateWithClientGeneratedId()
    {
        await using var db = CreateDb();
        var paper = new ExamPaper { Id = Guid.NewGuid(), Code = "P1", Title = "Paper" };
        var session = new ExamSession { Id = Guid.NewGuid(), Code = "S1", Title = "Session", ExamPaperId = paper.Id, ExamPaper = paper };
        var student = new Student { Id = Guid.NewGuid(), StudentCode = "SE180002", FullName = "Student Two" };
        db.AddRange(paper, session, student);
        await db.SaveChangesAsync();

        var controller = new ExamSessionsController(db);
        var response = await controller.AddCandidates(session.Id, new AddCandidatesRequest([student.Id]), CancellationToken.None);

        Assert.IsType<OkObjectResult>(response.Result);
        var candidate = Assert.Single(await db.ExamCandidates.ToListAsync());
        Assert.NotEqual(Guid.Empty, candidate.Id);
        Assert.Equal(student.Id, candidate.StudentId);
    }

    [Fact]
    public async Task CreateExamPaper_RejectsTestCasesJsonThatIsNotAnArray()
    {
        await using var db = CreateDb();
        var controller = new ExamPapersController(db);
        var request = new UpsertExamPaperRequest(
            "P1", "Paper", "v1", 10, ".*", true, true, 30, [],
            [new UpsertExamSectionRequest("API", 10, "Root", "{\"name\":\"not-an-array\"}", "Api/Api.csproj")]);

        var response = await controller.Create(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Contains("JSON array", badRequest.Value?.ToString());
        Assert.Empty(await db.ExamPapers.ToListAsync());
    }

    private static ExamAccountDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ExamAccountDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new ExamAccountDbContext(options);
    }
}
