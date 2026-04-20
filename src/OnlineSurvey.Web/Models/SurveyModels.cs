namespace OnlineSurvey.Web.Models;

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

public record SurveyResultResponse(
    Guid SurveyId,
    string SurveyTitle,
    int TotalResponses,
    List<QuestionResultResponse> Questions
);

public record QuestionResultResponse(
    Guid QuestionId,
    string QuestionText,
    List<OptionResultResponse> Options
);

public record OptionResultResponse(
    Guid OptionId,
    string OptionText,
    int Count,
    double Percentage
);

public record SubmitResponseRequest(
    Guid SurveyId,
    string? ParticipantId,
    List<AnswerRequest> Answers
);

public record AnswerRequest(
    Guid QuestionId,
    Guid SelectedOptionId
);

public enum SurveyStatus
{
    Draft = 0,
    Active = 1,
    Closed = 2
}

public enum SurveyAccessMode
{
    Anonymous = 0,
    CodeByEmail = 1,
    RequiresLogin = 2
}

[Flags]
public enum SurveyCollectedFields
{
    None  = 0,
    Name  = 1,
    Email = 2,
    Age   = 4
}

// Request models for creating surveys
public record CreateSurveyRequest(
    string Title,
    string? Description,
    List<CreateQuestionRequest> Questions,
    SurveyAccessMode AccessMode = SurveyAccessMode.Anonymous,
    SurveyCollectedFields CollectedFields = SurveyCollectedFields.None,
    bool IsPublic = true
);

public record RequestAccessCodeRequest(string Email);
public record VerifyAccessCodeRequest(string Email, string Code);
public record AccessCodeVerificationResponse(bool Success, string? Message = null);

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

public record ActivateSurveyRequest(
    DateTime? StartDate,
    DateTime? EndDate
);

public record UpdateSurveyRequest(
    string Title,
    string? Description
);
