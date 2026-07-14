using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PRN232.ExamAccount.Api.Controllers;
using PRN232.ExamAccount.Api.GraphQL.Queries;
using PRN232.ExamAccount.Application;
using PRN232.ExamAccount.Domain.Entities;
using PRN232.ExamAccount.Domain.Enums;
using PRN232.ExamAccount.Infrastructure;
using PRN232.ExamAccount.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddGraphQLServer()
    .AddQueryType<ExamQuery>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ExamAccountDbContext>();
    dbContext.Database.Migrate();
    SeedDefaultData(dbContext);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapGraphQL("/graphql");

app.Run();

static void SeedDefaultData(ExamAccountDbContext dbContext)
{
    if (dbContext.Students.Any())
    {
        return;
    }

    var admin = new StudentAccount
    {
        Id = Guid.NewGuid(),
        UserName = "admin",
        PasswordHash = AuthController.HashPassword("123456"),
        StudentCode = "ADMIN",
        FullName = "System Admin",
        Email = "admin@local",
        Role = UserRole.Admin,
        IsActive = true
    };

    var lecturer = new StudentAccount
    {
        Id = Guid.NewGuid(),
        UserName = "lecturer1",
        PasswordHash = AuthController.HashPassword("123456"),
        StudentCode = "LECT001",
        FullName = "Lecturer One",
        Email = "lecturer1@local",
        Role = UserRole.Lecturer,
        IsActive = true
    };

    var student = new StudentAccount
    {
        Id = Guid.NewGuid(),
        UserName = "student1",
        PasswordHash = AuthController.HashPassword("123456"),
        StudentCode = "SE000001",
        FullName = "Student One",
        Email = "student1@local",
        Role = UserRole.Student,
        IsActive = true
    };

    dbContext.Students.AddRange(admin, lecturer, student);
    dbContext.SaveChanges();

    dbContext.ExamRooms.Add(new ExamRoom
    {
        Id = Guid.NewGuid(),
        Code = "ROOM-1",
        Name = "Default Room",
        LecturerId = lecturer.Id
    });
    dbContext.SaveChanges();
}
