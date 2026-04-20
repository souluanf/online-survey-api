using OnlineSurvey.Domain.Enums;

namespace OnlineSurvey.Application.DTOs;

public record CreateSurveyRequest(
    string Title,
    string? Description,
    List<CreateQuestionRequest> Questions,
    SurveyAccessMode AccessMode = SurveyAccessMode.Anonymous,
    SurveyCollectedFields CollectedFields = SurveyCollectedFields.None,
    bool IsPublic = true
);

public record CreateQuestionRequest(
    string Text,
    int Order,
    bool IsRequired,
    List<CreateOptionRequest> Options
);

public record CreateOptionRequest(
    string Text,
    int Order
);

public record UpdateSurveyRequest(
    string Title,
    string? Description
);

public record ActivateSurveyRequest(
    DateTime? StartDate,
    DateTime? EndDate
);

public record SurveyResponse(
    Guid Id,
    string Title,
    string? Description,
    SurveyStatus Status,
    DateTime? StartDate,
    DateTime? EndDate,
    int QuestionCount,
    int ResponseCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    SurveyAccessMode AccessMode = SurveyAccessMode.Anonymous,
    SurveyCollectedFields CollectedFields = SurveyCollectedFields.None,
    bool IsPublic = true
);

public record SurveyDetailResponse(
    Guid Id,
    string Title,
    string? Description,
    SurveyStatus Status,
    DateTime? StartDate,
    DateTime? EndDate,
    List<QuestionResponse> Questions,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    SurveyAccessMode AccessMode = SurveyAccessMode.Anonymous,
    SurveyCollectedFields CollectedFields = SurveyCollectedFields.None,
    bool IsPublic = true
);

public record QuestionResponse(
    Guid Id,
    string Text,
    int Order,
    bool IsRequired,
    List<OptionResponse> Options
);

public record OptionResponse(
    Guid Id,
    string Text,
    int Order
);

public record PaginatedResponse<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);

public record RequestAccessCodeRequest(string Email);
public record VerifyAccessCodeRequest(string Email, string Code);
public record AccessCodeVerificationResponse(bool Success, string? Message = null);
