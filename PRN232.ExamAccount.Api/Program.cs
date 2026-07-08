using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PRN232.ExamAccount.Api.GraphQL.Queries;
using PRN232.ExamAccount.Application;
using PRN232.ExamAccount.Infrastructure;
using PRN232.ExamAccount.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

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
    dbContext.Database.ExecuteSqlRaw("CREATE SCHEMA IF NOT EXISTS exam;");
    
    var databaseCreator = dbContext.Database.GetService<Microsoft.EntityFrameworkCore.Storage.IDatabaseCreator>() 
        as Microsoft.EntityFrameworkCore.Storage.RelationalDatabaseCreator;
    if (databaseCreator != null)
    {
        var tableExists = false;
        try
        {
            var connection = dbContext.Database.GetDbConnection();
            var wasOpen = connection.State == System.Data.ConnectionState.Open;
            if (!wasOpen) connection.Open();
            try
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'exam' AND tablename = 'Exams');";
                    tableExists = (bool)(cmd.ExecuteScalar() ?? false);
                }
            }
            finally
            {
                if (!wasOpen) connection.Close();
            }
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"[DB Check Error] {ex.Message}");
        }

        if (!tableExists)
        {
            try
            {
                databaseCreator.CreateTables();
                System.Console.WriteLine("Database tables created successfully in exam schema.");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("==================================================");
                System.Console.WriteLine("DB CREATION ERROR IN EXAM SERVICE:");
                System.Console.WriteLine(ex.ToString());
                System.Console.WriteLine("==================================================");
            }
        }
        else
        {
            System.Console.WriteLine("Database tables already exist in exam schema. Skipping creation.");
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapGraphQL("/graphql");

app.Run();
