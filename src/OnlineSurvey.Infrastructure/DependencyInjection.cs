using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineSurvey.Application.Interfaces;
using OnlineSurvey.Domain.Repositories;
using OnlineSurvey.Infrastructure.Caching;
using OnlineSurvey.Infrastructure.Data;
using OnlineSurvey.Infrastructure.Email;
using OnlineSurvey.Infrastructure.Repositories;
using Resend;

namespace OnlineSurvey.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Only register PostgreSQL if connection string is available
        // This allows tests to replace with InMemory database
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(3);
                    npgsqlOptions.CommandTimeout(30);
                })
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
        }

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ISurveyRepository, SurveyRepository>();
        services.AddScoped<IResponseRepository, ResponseRepository>();
        services.AddScoped<ISurveyAccessCodeRepository, SurveyAccessCodeRepository>();

        services.AddOptions();
        services.AddHttpClient<ResendClient>();
        services.Configure<ResendClientOptions>(options =>
        {
            options.ApiToken = configuration["RESEND_API_KEY"] ?? throw new InvalidOperationException("RESEND_API_KEY not configured");
        });
        services.AddTransient<IResend, ResendClient>();
        services.AddScoped<IEmailService, ResendEmailService>();

        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        return services;
    }
}
