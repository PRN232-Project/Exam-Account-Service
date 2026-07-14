using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PRN232.ExamAccount.Application.Dashboard;
using PRN232.ExamAccount.Application.Interfaces;
using PRN232.ExamAccount.Infrastructure.Dashboard;
using PRN232.ExamAccount.Infrastructure.Messaging;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace PRN232.ExamAccount.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(options =>
            configuration.GetSection(RabbitMqOptions.SectionName).Bind(options));
        services.Configure<NotificationGrpcOptions>(options =>
            configuration.GetSection(NotificationGrpcOptions.SectionName).Bind(options));
        services.AddDbContext<ExamAccountDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IExamDashboardReader, ExamDashboardReader>();
        services.AddScoped<IGradingJobPublisher, RabbitMqJobPublisher>();
        services.AddSingleton<NotificationGrpcClient>();
        services.AddHostedService<GradingResultConsumer>();

        return services;
    }
}
