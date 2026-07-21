using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PRN232.ExamAccount.Infrastructure.Persistence;

public class ExamAccountDbContextFactory : IDesignTimeDbContextFactory<ExamAccountDbContext>
{
    public ExamAccountDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "PRN232.ExamAccount.Api"));

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing DefaultConnection for ExamAccountDbContextFactory.");

        var optionsBuilder = new DbContextOptionsBuilder<ExamAccountDbContext>();
        optionsBuilder.UseNpgsql(
            connectionString,
            postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", "exam"));

        return new ExamAccountDbContext(optionsBuilder.Options);
    }
}
