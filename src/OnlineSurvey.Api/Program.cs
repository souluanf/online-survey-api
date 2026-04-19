using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OnlineSurvey.Api.Endpoints;
using OnlineSurvey.Api.Middlewares;
using OnlineSurvey.Application;
using OnlineSurvey.Infrastructure;
using OnlineSurvey.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Online Survey API",
        Version = "v1",
        Description = "API for managing online surveys and collecting responses at scale"
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

#pragma warning disable S5332 // HTTP is acceptable for localhost and internal Docker network
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5000", "http://localhost:5173", "http://web:80"];
#pragma warning restore S5332

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Online Survey API v1");
    options.RoutePrefix = string.Empty;
});

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var isInMemory = dbContext.Database.ProviderName?.Contains("InMemory") == true;

    if (isInMemory)
    {
        await dbContext.Database.EnsureCreatedAsync();
        logger.LogInformation("In-memory database created");
    }
    else
    {
        logger.LogInformation("Applying database migrations...");
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully");
    }
}

app.UseCors("AllowFrontend");

app.MapSurveyEndpoints();
app.MapResponseEndpoints();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithTags("Health")
    .WithName("HealthCheck");

await app.RunAsync();

public partial class Program
{
    protected Program() { }
}
