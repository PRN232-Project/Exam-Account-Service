using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Api.Security;
using PRN232.ExamAccount.Domain.Entities;
using PRN232.ExamAccount.Domain.Enums;
using PRN232.ExamAccount.Infrastructure.Persistence;
using PRN232.ExamAccount.Api.Integration;

namespace PRN232.ExamAccount.Api.Controllers;

[ApiController, Route("api/grading-batches"), Authorize(Roles = nameof(UserRole.Lecturer))]
public class ExecutionPackagesController(ExamAccountDbContext db) : ControllerBase
{
    [HttpPost("{id:guid}/execution-package")]
    public async Task<ActionResult<BatchExecutionPackageDto>> Create(Guid id, CancellationToken ct)
    {
        var lecturerId = User.CurrentUserId();
        var batch = await db.GradingBatches
            .Include(x => x.ExamSession!).ThenInclude(x => x.ExamPaper!).ThenInclude(x => x.Sections)
            .Include(x => x.Items).ThenInclude(x => x.ExamCandidate!).ThenInclude(x => x.Student)
            .SingleOrDefaultAsync(x => x.Id == id && x.LecturerId == lecturerId, ct);
        if (batch is null) return NotFound();
        if (batch.Status != GradingBatchStatus.InProgress) return BadRequest("Batch phải ở trạng thái InProgress trước khi cấp execution package.");

        var plainToken = BatchExecutionTokenService.Create();
        var expiresAtUtc = DateTime.UtcNow.AddHours(4);
        db.BatchExecutionTokens.Add(new BatchExecutionToken
        {
            Id = Guid.NewGuid(), GradingBatchId = batch.Id, IssuedByUserId = lecturerId,
            TokenHash = BatchExecutionTokenService.Hash(plainToken), ExpiresAtUtc = expiresAtUtc
        });
        await db.SaveChangesAsync(ct);

        var paper = batch.ExamSession!.ExamPaper!;
        var sections = paper.Sections.OrderBy(x => x.Name).Select(x => new ExecutionSectionDto(
            x.Name, x.Weight, x.TestFilter,
            DeserializeTestCases(x.TestCasesJson), x.ApiProjectPath)).ToList();
        var items = batch.Items.OrderBy(x => x.ExamCandidate!.Student!.StudentCode)
            .Select(x => new ExecutionItemDto(x.Id, x.ExamCandidate!.Student!.StudentCode, x.ExamCandidate.Student.FullName, x.ExamCandidate.PaperCode)).ToList();
        return Ok(new BatchExecutionPackageDto(
            batch.Id, batch.Code, batch.ExamSessionId, batch.ExamSession.Code,
            new ExecutionPaperDto(paper.Id, paper.Code, paper.RubricVersion, paper.MaxScore, paper.SolutionPattern,
                paper.RequireAppSettings, paper.ForbidHardcodedConnectionString, paper.TimeoutSeconds, paper.PlagiarismKeywords, sections),
            items, plainToken, expiresAtUtc, $"{Request.Scheme}://{Request.Host}"));
    }

    private static IReadOnlyList<JsonElement> DeserializeTestCases(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<JsonElement>>(json) ?? []; }
        catch (JsonException) { return []; }
    }
}

[ApiController, Route("api/integration/grading-items"), AllowAnonymous]
public class GradingIntegrationController(ExamAccountDbContext db, RealtimeNotificationClient realtime) : ControllerBase
{
    [HttpPost("{id:guid}/match")]
    public async Task<IActionResult> Match(Guid id, MatchItemRequest request, CancellationToken ct)
    {
        var auth = await Authenticate(id, ct); if (auth.Error is not null) return auth.Error;
        var item = auth.Item!;
        item.Status = request.Found ? GradingItemStatus.LocalMatched : GradingItemStatus.MissingSubmission;
        item.LastErrorCode = request.Found ? "" : "LOCAL_SUBMISSION_NOT_FOUND";
        item.LastErrorMessage = request.Found ? "" : request.Note ?? "Không tìm thấy bài trên máy giảng viên.";
        if (request.Found) item.PlagiarismStatus = "Pending";
        await db.SaveChangesAsync(ct); return Ok(new { item.Id, item.Status });
    }

    [HttpPost("{id:guid}/attempts")]
    public async Task<ActionResult<object>> AddAttempt(Guid id, CreateAttemptRequest request, CancellationToken ct)
    {
        var auth = await Authenticate(id, ct); if (auth.Error is not null) return auth.Error;
        var duplicate = await db.GradingAttempts.AsNoTracking().SingleOrDefaultAsync(x => x.ClientRequestId == request.ClientRequestId, ct);
        if (duplicate is not null) return Ok(new { duplicate.Id, duplicate.AttemptNumber, idempotent = true });
        var item = auth.Item!; var batch = item.GradingBatch!; var paper = batch.ExamSession!.ExamPaper!;
        if (request.RubricVersion != paper.RubricVersion) return BadRequest($"RubricVersion phải là {paper.RubricVersion}.");
        if (!request.HasTechnicalError && (request.TotalScore < 0 || request.TotalScore > paper.MaxScore)) return BadRequest("Điểm vượt phạm vi mã đề.");
        var attempt = new GradingAttempt
        {
            Id = Guid.NewGuid(), GradingItemId = item.Id, AttemptNumber = item.LatestAttemptNumber + 1,
            ClientRequestId = request.ClientRequestId, TotalScore = request.TotalScore,
            RawJsonReport = request.RawJsonReport ?? "", HasTechnicalError = request.HasTechnicalError,
            ErrorCode = request.ErrorCode ?? "", ErrorMessage = request.ErrorMessage ?? "",
            RubricVersion = request.RubricVersion, CompletedAtUtc = request.CompletedAtUtc
        };
        db.GradingAttempts.Add(attempt); item.LatestAttemptNumber = attempt.AttemptNumber;
        item.LastErrorCode = attempt.ErrorCode; item.LastErrorMessage = attempt.ErrorMessage;
        NotificationRecord? notification = null;
        if (attempt.HasTechnicalError)
        {
            item.Status = GradingItemStatus.TechnicalError;
            notification = GradingBatchesController.NewNotification(batch.LecturerId, batch.Id, item.Id,
                "GradingTechnicalError", "Bài chấm bị lỗi kỹ thuật", attempt.ErrorMessage); db.Notifications.Add(notification);
        }
        else
        {
            item.LatestScore = attempt.TotalScore; item.Status = GradingItemStatus.Graded;
            foreach (var review in item.ReviewRequests.Where(x => !x.IsResolved)) { review.IsResolved = true; review.ResolvedAtUtc = DateTime.UtcNow; }
        }
        await db.SaveChangesAsync(ct);
        if (notification is not null) await realtime.SendAsync(notification, batch.ExamSessionId, batch.ExamSession!.RoomId, ct);
        return Ok(new { attempt.Id, attempt.AttemptNumber, item.Status });
    }

    [HttpPost("{id:guid}/plagiarism")]
    public async Task<IActionResult> SavePlagiarism(Guid id, PlagiarismCallbackRequest request, CancellationToken ct)
    {
        var auth = await Authenticate(id, ct); if (auth.Error is not null) return auth.Error;
        var item = auth.Item!; var batch = item.GradingBatch!;
        item.PlagiarismStatus = request.Status;
        item.PlagiarismViolationCount = request.ViolationCount;
        item.PlagiarismMaxSimilarity = request.MaxSimilarity;
        item.PlagiarismReportJson = string.IsNullOrWhiteSpace(request.RawJsonReport) ? "{}" : request.RawJsonReport;
        item.PlagiarismErrorMessage = request.ErrorMessage ?? "";
        item.PlagiarismCheckedAtUtc = request.CheckedAtUtc;

        var notifications = new List<NotificationRecord>();
        if (request.Status == "Completed" && (request.ViolationCount > 0 || request.MaxSimilarity >= 50))
        {
            var officers = await db.Users.Where(x => x.Role == UserRole.ExamOfficer && x.IsActive).Select(x => x.Id).ToListAsync(ct);
            foreach (var officerId in officers)
                notifications.Add(GradingBatchesController.NewNotification(officerId, batch.Id, item.Id, "PlagiarismAlert", "Phát hiện dấu hiệu đạo văn", $"Bài có {request.ViolationCount} vi phạm, tương đồng cao nhất {request.MaxSimilarity ?? 0:0.##}%."));
        }
        else if (request.Status == "TechnicalError")
            notifications.Add(GradingBatchesController.NewNotification(batch.LecturerId, batch.Id, item.Id, "PlagiarismTechnicalError", "Quét đạo văn bị lỗi", item.PlagiarismErrorMessage));
        db.Notifications.AddRange(notifications);
        await db.SaveChangesAsync(ct);
        foreach (var notification in notifications) await realtime.SendAsync(notification, batch.ExamSessionId, batch.ExamSession!.RoomId, ct);
        return Ok(new { item.Id, item.PlagiarismStatus, item.PlagiarismViolationCount, item.PlagiarismMaxSimilarity });
    }

    private async Task<(GradingItem? Item, ActionResult? Error)> Authenticate(Guid itemId, CancellationToken ct)
    {
        var authorization = Request.Headers.Authorization.ToString();
        if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return (null, Unauthorized("Thiếu execution token."));
        var plainToken = authorization[7..].Trim(); if (plainToken.Length == 0) return (null, Unauthorized("Execution token rỗng."));
        var tokenHash = BatchExecutionTokenService.Hash(plainToken);
        var token = await db.BatchExecutionTokens.AsNoTracking().SingleOrDefaultAsync(x => x.TokenHash == tokenHash, ct);
        if (token is null || token.RevokedAtUtc is not null || token.ExpiresAtUtc <= DateTime.UtcNow) return (null, Unauthorized("Execution token không hợp lệ hoặc đã hết hạn."));
        var item = await db.GradingItems.Include(x => x.Attempts).Include(x => x.ReviewRequests)
            .Include(x => x.GradingBatch).ThenInclude(x => x!.ExamSession).ThenInclude(x => x!.ExamPaper)
            .SingleOrDefaultAsync(x => x.Id == itemId && x.GradingBatchId == token.GradingBatchId, ct);
        if (item is null) return (null, NotFound());
        if (item.GradingBatch!.Status != GradingBatchStatus.InProgress) return (null, BadRequest("Batch không ở trạng thái InProgress."));
        return (item, null);
    }
}

public record ExecutionSectionDto(string Name, decimal Weight, string TestFilter, IReadOnlyList<JsonElement> TestCases, string ApiProjectPath);
public record ExecutionPaperDto(Guid Id, string Code, string RubricVersion, decimal MaxScore, string SolutionPattern, bool RequireAppSettings, bool ForbidHardcodedConnectionString, int TimeoutSeconds, string[] PlagiarismKeywords, IReadOnlyList<ExecutionSectionDto> Sections);
public record ExecutionItemDto(Guid GradingItemId, string StudentCode, string StudentName, string PaperCode);
public record BatchExecutionPackageDto(Guid BatchId, string BatchCode, Guid ExamSessionId, string ExamSessionCode, ExecutionPaperDto ExamPaper, IReadOnlyList<ExecutionItemDto> Items, string ExecutionToken, DateTime ExpiresAtUtc, string CentralApiBaseUrl);
public record PlagiarismCallbackRequest(string Status, int ViolationCount, decimal? MaxSimilarity, string? RawJsonReport, string? ErrorMessage, DateTime CheckedAtUtc);
