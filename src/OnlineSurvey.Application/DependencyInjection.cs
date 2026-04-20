using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OnlineSurvey.Application.Interfaces;
using OnlineSurvey.Application.Services;

namespace OnlineSurvey.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISurveyService, SurveyService>();

        // Register ResponseService as concrete type for decorator
        services.AddScoped<ResponseService>();
        services.AddScoped<IResponseService, CachedResponseService>();

        services.AddScoped<IAccessCodeService, AccessCodeService>();

        services.AddValidatorsFromAssemblyContaining<ISurveyService>();

        return services;
    }
}
