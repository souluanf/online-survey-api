using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OnlineSurvey.Application.DTOs;
using OnlineSurvey.Application.Interfaces;
using OnlineSurvey.Domain.Enums;


namespace OnlineSurvey.Api.Endpoints;

public static class SurveyEndpoints
{
    public static void MapSurveyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/surveys")
            .WithTags("Surveys");

        group.MapPost("/", CreateSurvey)
            .WithName("CreateSurvey")
            .WithDescription("Creates a new survey with questions and options")
            .Produces<SurveyDetailResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .RequireAuthorization();

        group.MapGet("/", GetSurveys)
            .WithName("GetSurveys")
            .WithDescription("Gets paginated list of surveys owned by the authenticated user")
            .Produces<PaginatedResponse<SurveyResponse>>()
            .RequireAuthorization();

        group.MapGet("/active", GetActiveSurveys)
            .WithName("GetActiveSurveys")
            .WithDescription("Gets all active surveys available for responses")
            .Produces<IEnumerable<SurveyResponse>>();

        group.MapGet("/{id:guid}", GetSurveyById)
            .WithName("GetSurveyById")
            .WithDescription("Gets a survey by ID with all questions and options")
            .Produces<SurveyDetailResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdateSurvey)
            .WithName("UpdateSurvey")
            .WithDescription("Updates survey title and description (draft only)")
            .Produces<SurveyDetailResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .RequireAuthorization();

        group.MapPost("/{id:guid}/activate", ActivateSurvey)
            .WithName("ActivateSurvey")
            .WithDescription("Activates a draft survey to start collecting responses")
            .Produces<SurveyDetailResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .RequireAuthorization();

        group.MapPost("/{id:guid}/close", CloseSurvey)
            .WithName("CloseSurvey")
            .WithDescription("Closes an active survey")
            .Produces<SurveyDetailResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        group.MapDelete("/{id:guid}", DeleteSurvey)
            .WithName("DeleteSurvey")
            .WithDescription("Deletes a survey (not active)")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        group.MapPost("/{id:guid}/access/request", RequestAccessCode)
            .WithName("RequestAccessCode")
            .WithDescription("Requests an access code for a survey by email")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/access/verify", VerifyAccessCode)
            .WithName("VerifyAccessCode")
            .WithDescription("Verifies an access code for a survey")
            .Produces<AccessCodeVerificationResponse>();
    }

    private static async Task<IResult> CreateSurvey(
        [FromBody] CreateSurveyRequest request,
        HttpContext httpContext,
        ISurveyService surveyService,
        IValidator<CreateSurveyRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var ownerId = httpContext.User.FindFirst("user_id")?.Value ?? "";
        var survey = await surveyService.CreateSurveyAsync(request, ownerId, cancellationToken);
        return Results.Created($"/api/surveys/{survey.Id}", survey);
    }

    private static async Task<IResult> GetSurveys(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] SurveyStatus? status,
        HttpContext httpContext,
        ISurveyService surveyService,
        CancellationToken cancellationToken)
    {
        var effectivePage = page is null or <= 0 ? 1 : page.Value;
        var effectivePageSize = pageSize is null or <= 0 ? 10 : Math.Min(pageSize.Value, 100);

        var ownerId = httpContext.User.FindFirst("user_id")?.Value ?? "";
        var result = await surveyService.GetSurveysAsync(effectivePage, effectivePageSize, ownerId, status, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetActiveSurveys(
        ISurveyService surveyService,
        CancellationToken cancellationToken)
    {
        var surveys = await surveyService.GetActiveSurveysAsync(cancellationToken);
        return Results.Ok(surveys);
    }

    private static async Task<IResult> GetSurveyById(
        Guid id,
        ISurveyService surveyService,
        CancellationToken cancellationToken)
    {
        var survey = await surveyService.GetSurveyByIdAsync(id, cancellationToken);
        return survey is null ? Results.NotFound() : Results.Ok(survey);
    }

    private static async Task<IResult> UpdateSurvey(
        Guid id,
        [FromBody] UpdateSurveyRequest request,
        ISurveyService surveyService,
        CancellationToken cancellationToken)
    {
        var survey = await surveyService.UpdateSurveyAsync(id, request, cancellationToken);
        return Results.Ok(survey);
    }

    private static async Task<IResult> ActivateSurvey(
        Guid id,
        [FromBody] ActivateSurveyRequest request,
        ISurveyService surveyService,
        CancellationToken cancellationToken)
    {
        var survey = await surveyService.ActivateSurveyAsync(id, request, cancellationToken);
        return Results.Ok(survey);
    }

    private static async Task<IResult> CloseSurvey(
        Guid id,
        ISurveyService surveyService,
        CancellationToken cancellationToken)
    {
        var survey = await surveyService.CloseSurveyAsync(id, cancellationToken);
        return Results.Ok(survey);
    }

    private static async Task<IResult> DeleteSurvey(
        Guid id,
        ISurveyService surveyService,
        CancellationToken cancellationToken)
    {
        await surveyService.DeleteSurveyAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> RequestAccessCode(
        Guid id,
        [FromBody] RequestAccessCodeRequest request,
        IAccessCodeService accessCodeService,
        CancellationToken cancellationToken)
    {
        var success = await accessCodeService.RequestCodeAsync(id, request.Email, cancellationToken);
        return success ? Results.Ok() : Results.NotFound();
    }

    private static async Task<IResult> VerifyAccessCode(
        Guid id,
        [FromBody] VerifyAccessCodeRequest request,
        IAccessCodeService accessCodeService,
        CancellationToken cancellationToken)
    {
        var success = await accessCodeService.VerifyCodeAsync(id, request.Email, request.Code, cancellationToken);
        return Results.Ok(new AccessCodeVerificationResponse(success, success ? null : "Código inválido ou expirado."));
    }
}
