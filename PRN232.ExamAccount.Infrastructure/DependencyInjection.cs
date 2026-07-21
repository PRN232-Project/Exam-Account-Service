using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ExamAccountDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", "exam")));
        return services;
    }
}
