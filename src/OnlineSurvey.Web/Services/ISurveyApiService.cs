using OnlineSurvey.Web.Models;

namespace OnlineSurvey.Web.Services;

public interface ISurveyApiService
{
    Task<IEnumerable<SurveyResponse>> GetActiveSurveysAsync();
    Task<PaginatedResponse<SurveyResponse>?> GetSurveysAsync(int page = 1, int pageSize = 50, SurveyStatus? status = null);
    Task<SurveyDetailResponse?> GetSurveyByIdAsync(Guid id);
    Task<SurveyResultResponse?> GetSurveyResultsAsync(Guid surveyId);
    Task<bool> SubmitResponseAsync(SubmitResponseRequest request);
    Task<SurveyDetailResponse?> CreateSurveyAsync(CreateSurveyRequest request);
    Task<SurveyDetailResponse?> ActivateSurveyAsync(Guid surveyId, ActivateSurveyRequest request);
    Task<bool> DeleteSurveyAsync(Guid surveyId);
    Task<SurveyDetailResponse?> UpdateSurveyAsync(Guid surveyId, UpdateSurveyRequest request);
    Task<SurveyDetailResponse?> CloseSurveyAsync(Guid surveyId);
    Task<bool> RequestAccessCodeAsync(Guid surveyId, string email);
    Task<bool> VerifyAccessCodeAsync(Guid surveyId, string email, string code);
}
