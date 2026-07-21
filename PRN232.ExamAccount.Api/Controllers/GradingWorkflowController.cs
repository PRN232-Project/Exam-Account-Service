using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Api.Security;
using PRN232.ExamAccount.Domain.Entities;
using PRN232.ExamAccount.Domain.Enums;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Api.Controllers;

[ApiController, Route("api/grading-batches"), Authorize]
public class GradingBatchesController(ExamAccountDbContext db) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = nameof(UserRole.ExamOfficer))]
    public Task<List<BatchListDto>> GetAll(CancellationToken ct) => Query().OrderByDescending(x => x.AssignedAtUtc).Select(MapList()).ToListAsync(ct);

    [HttpGet("mine")]
    [Authorize(Roles = nameof(UserRole.Lecturer))]
    public Task<List<BatchListDto>> Mine(CancellationToken ct) { var id = User.CurrentUserId(); return Query().Where(x => x.LecturerId == id).OrderByDescending(x => x.AssignedAtUtc).Select(MapList()).ToListAsync(ct); }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BatchDetailDto>> Detail(Guid id, CancellationToken ct)
    {
        var x = await Query().Include(b => b.ExamSession!).ThenInclude(s => s.ExamPaper!).ThenInclude(p => p.Sections)
            .Include(b => b.Items).ThenInclude(i => i.ExamCandidate!).ThenInclude(c => c.Student)
            .Include(b => b.Items).ThenInclude(i => i.Attempts)
            .Include(b => b.Items).ThenInclude(i => i.ReviewRequests)
            .SingleOrDefaultAsync(b => b.Id == id, ct);
        if (x is null) return NotFound(); if (User.IsInRole(nameof(UserRole.Lecturer)) && x.LecturerId != User.CurrentUserId()) return Forbid();
        var paper = x.ExamSession!.ExamPaper!;
        return Ok(new BatchDetailDto(x.Id, x.Code, x.Status, x.ExamSessionId, x.ExamSession.Code, paper.Code, paper.RubricVersion, paper.MaxScore, paper.SolutionPattern, paper.RequireAppSettings, paper.ForbidHardcodedConnectionString, paper.TimeoutSeconds,
            paper.Sections.Select(s => new ExamSectionDto(s.Id, s.Name, s.Weight, s.TestFilter)).ToList(),
            x.Items.OrderBy(i => i.ExamCandidate!.Student!.StudentCode).Select(i => new GradingItemDto(i.Id, i.ExamCandidateId, i.ExamCandidate!.Student!.StudentCode, i.ExamCandidate.Student.FullName, i.ExamCandidate.PaperCode, i.Status, i.LatestScore, i.LatestAttemptNumber, i.LastErrorCode, i.LastErrorMessage, i.ReviewRequests.Where(r => !r.IsResolved).OrderByDescending(r => r.CreatedAtUtc).Select(r => r.Reason).FirstOrDefault())).ToList()));
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.ExamOfficer))]
    public async Task<ActionResult<object>> Create(CreateBatchRequest r, CancellationToken ct)
    {
        if (await db.GradingBatches.AnyAsync(x => x.Code == r.Code.Trim(), ct)) return Conflict("Mã batch đã tồn tại.");
        var lecturer = await db.Users.SingleOrDefaultAsync(x => x.Id == r.LecturerId && x.Role == UserRole.Lecturer && x.IsActive, ct); if (lecturer is null) return BadRequest("Giảng viên không hợp lệ.");
        var session = await db.ExamSessions.Include(x => x.Candidates).SingleOrDefaultAsync(x => x.Id == r.ExamSessionId, ct); if (session is null || session.Status != ExamSessionStatus.Ready) return BadRequest("Ca thi phải tồn tại và ở trạng thái Ready.");
        var selected = r.ExamCandidateIds.Count == 0 ? session.Candidates.ToList() : session.Candidates.Where(x => r.ExamCandidateIds.Contains(x.Id)).ToList();
        if (selected.Count == 0 || selected.Any(x => x.IsAbsent) || (r.ExamCandidateIds.Count > 0 && selected.Count != r.ExamCandidateIds.Distinct().Count())) return BadRequest("Danh sách candidate không hợp lệ hoặc có sinh viên vắng.");
        var already = await db.GradingItems.Where(x => selected.Select(c => c.Id).Contains(x.ExamCandidateId)).Select(x => x.ExamCandidateId).ToListAsync(ct); if (already.Count > 0) return Conflict("Một hoặc nhiều candidate đã được phân công.");
        var batch = new GradingBatch { Id = Guid.NewGuid(), Code = r.Code.Trim(), ExamSessionId = session.Id, LecturerId = lecturer.Id, Status = GradingBatchStatus.Assigned };
        foreach (var candidate in selected) batch.Items.Add(new GradingItem { Id = Guid.NewGuid(), ExamCandidateId = candidate.Id, Status = GradingItemStatus.Assigned });
        db.Add(batch); db.Notifications.Add(NewNotification(lecturer.Id, batch.Id, null, "BatchAssigned", "Có đợt chấm mới", $"Bạn được phân công đợt {batch.Code}.")); await db.SaveChangesAsync(ct);
        return Created("api/grading-batches/" + batch.Id, new { batch.Id, batch.Code, batch.Status, itemCount = batch.Items.Count });
    }

    [HttpPost("{id:guid}/start")]
    [Authorize(Roles = nameof(UserRole.Lecturer))]
    public async Task<IActionResult> Start(Guid id, CancellationToken ct) { var x = await OwnedBatch(id, ct); if (x is null) return NotFound(); if (x.Status is not (GradingBatchStatus.Assigned or GradingBatchStatus.NeedsCorrection)) return BadRequest("Batch không thể bắt đầu ở trạng thái hiện tại."); x.Status = GradingBatchStatus.InProgress; x.ExamSession!.Status = ExamSessionStatus.Grading; await db.SaveChangesAsync(ct); return Ok(new { x.Id, x.Status }); }

    [HttpPost("{id:guid}/submit")]
    [Authorize(Roles = nameof(UserRole.Lecturer))]
    public async Task<IActionResult> Submit(Guid id, CancellationToken ct) { var x = await OwnedBatch(id, ct, true); if (x is null) return NotFound(); if (x.Status != GradingBatchStatus.InProgress) return BadRequest("Batch chưa ở InProgress."); if (x.Items.Any(i => i.Status is not (GradingItemStatus.Graded or GradingItemStatus.Submitted or GradingItemStatus.MissingSubmission))) return BadRequest("Mọi bài phải Graded, Submitted hoặc MissingSubmission trước khi gửi."); foreach (var i in x.Items) if (i.Status == GradingItemStatus.Graded) i.Status = GradingItemStatus.Submitted; x.Status = x.SubmittedAtUtc is null ? GradingBatchStatus.SubmittedForReview : GradingBatchStatus.Resubmitted; x.SubmittedAtUtc = DateTime.UtcNow; x.ExamSession!.Status = ExamSessionStatus.UnderReview; await db.SaveChangesAsync(ct); return Ok(new { x.Id, x.Status }); }

    [HttpPost("{id:guid}/accept")]
    [Authorize(Roles = nameof(UserRole.ExamOfficer))]
    public async Task<IActionResult> Accept(Guid id, CancellationToken ct) { var x = await db.GradingBatches.Include(b => b.Items).Include(b => b.ExamSession).SingleOrDefaultAsync(b => b.Id == id, ct); if (x is null) return NotFound(); if (x.Status is not (GradingBatchStatus.SubmittedForReview or GradingBatchStatus.Resubmitted)) return BadRequest("Batch chưa được gửi để duyệt."); if (x.Items.Any(i => i.Status is not (GradingItemStatus.Submitted or GradingItemStatus.MissingSubmission))) return BadRequest("Batch còn item chưa sẵn sàng."); foreach (var i in x.Items.Where(i => i.Status == GradingItemStatus.Submitted)) i.Status = GradingItemStatus.Accepted; x.Status = GradingBatchStatus.Accepted; x.AcceptedAtUtc = DateTime.UtcNow; x.ExamSession!.Status = ExamSessionStatus.Completed; await db.SaveChangesAsync(ct); return Ok(new { x.Id, x.Status }); }

    private IQueryable<GradingBatch> Query() => db.GradingBatches.AsNoTracking().Include(x => x.Lecturer).Include(x => x.ExamSession).ThenInclude(x => x!.ExamPaper).Include(x => x.Items);
    private async Task<GradingBatch?> OwnedBatch(Guid id, CancellationToken ct, bool includeItems = false) { IQueryable<GradingBatch> q = db.GradingBatches.Include(x => x.ExamSession); if (includeItems) q = q.Include(x => x.Items); var userId = User.CurrentUserId(); return await q.SingleOrDefaultAsync(x => x.Id == id && x.LecturerId == userId, ct); }
    private static System.Linq.Expressions.Expression<Func<GradingBatch, BatchListDto>> MapList() => x => new BatchListDto(x.Id, x.Code, x.Status, x.ExamSessionId, x.ExamSession!.Code, x.ExamSession.ExamPaper!.Code, x.LecturerId, x.Lecturer!.FullName, x.Items.Count, x.Items.Count(i => i.Status == GradingItemStatus.Graded || i.Status == GradingItemStatus.Submitted || i.Status == GradingItemStatus.Accepted), x.AssignedAtUtc);
    internal static NotificationRecord NewNotification(Guid userId, Guid? batchId, Guid? itemId, string type, string title, string message) => new() { Id = Guid.NewGuid(), RecipientUserId = userId, GradingBatchId = batchId, GradingItemId = itemId, Type = type, Title = title, Message = message };
}

[ApiController, Route("api/grading-items"), Authorize]
public class GradingItemsController(ExamAccountDbContext db) : ControllerBase
{
    [HttpPost("{id:guid}/match")]
    [Authorize(Roles = nameof(UserRole.Lecturer))]
    public async Task<IActionResult> Match(Guid id, MatchItemRequest r, CancellationToken ct) { var x = await OwnedItem(id, ct); if (x is null) return NotFound(); if (x.GradingBatch!.Status != GradingBatchStatus.InProgress) return BadRequest("Batch chưa ở InProgress."); x.Status = r.Found ? GradingItemStatus.LocalMatched : GradingItemStatus.MissingSubmission; x.LastErrorCode = r.Found ? "" : "LOCAL_SUBMISSION_NOT_FOUND"; x.LastErrorMessage = r.Found ? "" : (r.Note ?? "Không tìm thấy bài trên máy giảng viên."); await db.SaveChangesAsync(ct); return Ok(new { x.Id, x.Status }); }

    [HttpPost("{id:guid}/attempts")]
    [Authorize(Roles = nameof(UserRole.Lecturer))]
    public async Task<ActionResult<object>> AddAttempt(Guid id, CreateAttemptRequest r, CancellationToken ct)
    {
        var duplicate = await db.GradingAttempts.AsNoTracking().SingleOrDefaultAsync(x => x.ClientRequestId == r.ClientRequestId, ct); if (duplicate is not null) return Ok(new { duplicate.Id, duplicate.AttemptNumber, idempotent = true });
        var x = await OwnedItem(id, ct); if (x is null) return NotFound(); if (x.GradingBatch!.Status != GradingBatchStatus.InProgress) return BadRequest("Batch chưa ở InProgress.");
        var expectedVersion = x.GradingBatch.ExamSession!.ExamPaper!.RubricVersion; if (r.RubricVersion != expectedVersion) return BadRequest($"RubricVersion phải là {expectedVersion}.");
        var attempt = new GradingAttempt { Id = Guid.NewGuid(), GradingItemId = x.Id, AttemptNumber = x.LatestAttemptNumber + 1, ClientRequestId = r.ClientRequestId, TotalScore = r.TotalScore, RawJsonReport = r.RawJsonReport ?? "", HasTechnicalError = r.HasTechnicalError, ErrorCode = r.ErrorCode ?? "", ErrorMessage = r.ErrorMessage ?? "", RubricVersion = r.RubricVersion, CompletedAtUtc = r.CompletedAtUtc };
        x.Attempts.Add(attempt); x.LatestAttemptNumber = attempt.AttemptNumber; x.LastErrorCode = attempt.ErrorCode; x.LastErrorMessage = attempt.ErrorMessage;
        if (r.HasTechnicalError) { x.Status = GradingItemStatus.TechnicalError; db.Notifications.Add(GradingBatchesController.NewNotification(x.GradingBatch.LecturerId, x.GradingBatchId, x.Id, "GradingTechnicalError", "Bài chấm bị lỗi kỹ thuật", attempt.ErrorMessage)); }
        else { if (r.TotalScore < 0 || r.TotalScore > x.GradingBatch.ExamSession.ExamPaper.MaxScore) return BadRequest("Điểm vượt phạm vi mã đề."); x.LatestScore = r.TotalScore; x.Status = GradingItemStatus.Graded; foreach (var review in x.ReviewRequests.Where(y => !y.IsResolved)) { review.IsResolved = true; review.ResolvedAtUtc = DateTime.UtcNow; } }
        await db.SaveChangesAsync(ct); return Ok(new { attempt.Id, attempt.AttemptNumber, x.Status });
    }

    [HttpPost("{id:guid}/retry")]
    [Authorize(Roles = nameof(UserRole.Lecturer))]
    public async Task<IActionResult> Retry(Guid id, CancellationToken ct) { var x = await OwnedItem(id, ct); if (x is null) return NotFound(); if (x.Status is not (GradingItemStatus.TechnicalError or GradingItemStatus.ReturnedForCorrection or GradingItemStatus.MissingSubmission)) return BadRequest("Item không ở trạng thái có thể retry."); x.Status = GradingItemStatus.LocalMatched; x.LastErrorCode = x.LastErrorMessage = ""; await db.SaveChangesAsync(ct); return Ok(new { x.Id, x.Status }); }

    [HttpPost("{id:guid}/return")]
    [Authorize(Roles = nameof(UserRole.ExamOfficer))]
    public async Task<IActionResult> Return(Guid id, ReturnItemRequest r, CancellationToken ct) { if (string.IsNullOrWhiteSpace(r.Reason)) return BadRequest("Phải nhập lý do trả bài."); var x = await db.GradingItems.Include(i => i.GradingBatch).SingleOrDefaultAsync(i => i.Id == id, ct); if (x is null) return NotFound(); if (x.Status != GradingItemStatus.Submitted) return BadRequest("Chỉ trả item đã Submitted."); x.Status = GradingItemStatus.ReturnedForCorrection; x.GradingBatch!.Status = GradingBatchStatus.NeedsCorrection; db.ReviewRequests.Add(new ReviewRequest { Id = Guid.NewGuid(), GradingItemId = id, RequestedByUserId = User.CurrentUserId(), Reason = r.Reason.Trim() }); db.Notifications.Add(GradingBatchesController.NewNotification(x.GradingBatch.LecturerId, x.GradingBatchId, x.Id, "ResultReturned", "Kết quả bị trả lại", r.Reason.Trim())); await db.SaveChangesAsync(ct); return Ok(new { x.Id, x.Status, batchStatus = x.GradingBatch.Status }); }

    private async Task<GradingItem?> OwnedItem(Guid id, CancellationToken ct) { var userId = User.CurrentUserId(); return await db.GradingItems.Include(x => x.ReviewRequests).Include(x => x.Attempts).Include(x => x.GradingBatch).ThenInclude(x => x!.ExamSession).ThenInclude(x => x!.ExamPaper).SingleOrDefaultAsync(x => x.Id == id && x.GradingBatch!.LecturerId == userId, ct); }
}

public record CreateBatchRequest(string Code, Guid ExamSessionId, Guid LecturerId, IReadOnlyList<Guid> ExamCandidateIds);
public record BatchListDto(Guid Id, string Code, GradingBatchStatus Status, Guid ExamSessionId, string ExamSessionCode, string ExamPaperCode, Guid LecturerId, string LecturerName, int ItemCount, int CompletedItemCount, DateTime AssignedAtUtc);
public record GradingItemDto(Guid Id, Guid ExamCandidateId, string StudentCode, string StudentName, string PaperCode, GradingItemStatus Status, decimal? LatestScore, int LatestAttemptNumber, string LastErrorCode, string LastErrorMessage, string? ActiveReviewReason);
public record BatchDetailDto(Guid Id, string Code, GradingBatchStatus Status, Guid ExamSessionId, string ExamSessionCode, string ExamPaperCode, string RubricVersion, decimal MaxScore, string SolutionPattern, bool RequireAppSettings, bool ForbidHardcodedConnectionString, int TimeoutSeconds, IReadOnlyList<ExamSectionDto> Sections, IReadOnlyList<GradingItemDto> Items);
public record MatchItemRequest(bool Found, string? Note);
public record CreateAttemptRequest(string ClientRequestId, decimal TotalScore, string? RawJsonReport, bool HasTechnicalError, string? ErrorCode, string? ErrorMessage, string RubricVersion, DateTime CompletedAtUtc);
public record ReturnItemRequest(string Reason);
