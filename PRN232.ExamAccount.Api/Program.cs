using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PRN232.ExamAccount.Api.Security;
using PRN232.ExamAccount.Application;
using PRN232.ExamAccount.Domain.Entities;
using PRN232.ExamAccount.Domain.Enums;
using PRN232.ExamAccount.Infrastructure;
using PRN232.ExamAccount.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<JwtTokenService>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"], ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)), ClockSkew = TimeSpan.FromSeconds(30)
    };
});
builder.Services.AddAuthorization();
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:5173"]).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ExamAccountDbContext>();
    await db.Database.MigrateAsync();
    await SeedAsync(db);
}
app.UseSwagger(); app.UseSwaggerUI(); app.UseCors("Frontend"); app.UseHttpsRedirection(); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers(); app.Run();

static async Task SeedAsync(ExamAccountDbContext db)
{
    if (!await db.Users.AnyAsync())
    {
        db.Users.AddRange(
            NewUser("admin", "System Admin", "admin@local", UserRole.Admin),
            NewUser("examofficer1", "Exam Officer One", "examofficer1@local", UserRole.ExamOfficer),
            NewUser("lecturer1", "Lecturer One", "lecturer1@local", UserRole.Lecturer));
        await db.SaveChangesAsync();
    }
    var room = await db.ExamRooms.SingleOrDefaultAsync(x => x.Code == "ROOM-1");
    if (room is null) { room = new ExamRoom { Id = Guid.NewGuid(), Code = "ROOM-1", Name = "Phòng thi 1", Location = "Lab 1" }; db.Add(room); }
    var student = await db.Students.SingleOrDefaultAsync(x => x.StudentCode == "SE000001");
    if (student is null) { student = new Student { Id = Guid.NewGuid(), StudentCode = "SE000001", FullName = "Student One", Email = "student1@local", ClassName = "SE-DEMO" }; db.Add(student); }
    var paper = await db.ExamPapers.Include(x => x.Sections).SingleOrDefaultAsync(x => x.Code == "PRN223-DEMO");
    if (paper is null)
    {
        paper = new ExamPaper { Id = Guid.NewGuid(), Code = "PRN223-DEMO", Title = "PRN223 Practical Demo", RubricVersion = "1.0", MaxScore = 10, SolutionPattern = "*.sln", TimeoutSeconds = 15 };
        paper.Sections.Add(new ExamSectionDefinition { Id = Guid.NewGuid(), Name = "Unit Tests", Weight = 10, TestFilter = "FullyQualifiedName~Unit" }); db.Add(paper);
    }
    await db.SaveChangesAsync();
    var session = await db.ExamSessions.Include(x => x.Candidates).SingleOrDefaultAsync(x => x.Code == "PE-DEMO-2026");
    if (session is null)
    {
        session = new ExamSession { Id = Guid.NewGuid(), Code = "PE-DEMO-2026", Title = "Ca thi demo", RoomId = room.Id, ExamPaperId = paper.Id, ScheduledAtUtc = DateTime.UtcNow, Status = ExamSessionStatus.Ready };
        session.Candidates.Add(new ExamCandidate { Id = Guid.NewGuid(), StudentId = student.Id, PaperCode = paper.Code }); db.Add(session); await db.SaveChangesAsync();
    }
    var lecturer = await db.Users.SingleAsync(x => x.UserName == "lecturer1");
    if (!await db.GradingBatches.AnyAsync(x => x.Code == "BATCH-DEMO-001"))
    {
        var candidate = session.Candidates.Single(x => x.StudentId == student.Id);
        var batch = new GradingBatch { Id = Guid.NewGuid(), Code = "BATCH-DEMO-001", ExamSessionId = session.Id, LecturerId = lecturer.Id };
        batch.Items.Add(new GradingItem { Id = Guid.NewGuid(), ExamCandidateId = candidate.Id }); db.Add(batch); await db.SaveChangesAsync();
    }
}
static UserAccount NewUser(string name, string fullName, string email, UserRole role) => new() { Id = Guid.NewGuid(), UserName = name, FullName = fullName, Email = email, Role = role, PasswordHash = PasswordService.Hash("123456") };

public partial class Program { }
