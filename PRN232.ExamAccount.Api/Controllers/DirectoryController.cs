using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Domain.Enums;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Api.Controllers;

[ApiController, Route("api/directory"), Authorize(Roles = nameof(UserRole.ExamOfficer))]
public class DirectoryController(ExamAccountDbContext db) : ControllerBase
{
    [HttpGet("lecturers")]
    public Task<List<DirectoryUserDto>> Lecturers(CancellationToken ct) => db.Users.AsNoTracking()
        .Where(x => x.Role == UserRole.Lecturer && x.IsActive).OrderBy(x => x.FullName)
        .Select(x => new DirectoryUserDto(x.Id, x.FullName, x.Email)).ToListAsync(ct);
}
public record DirectoryUserDto(Guid Id, string FullName, string Email);
