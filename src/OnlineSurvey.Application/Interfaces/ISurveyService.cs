using OnlineSurvey.Application.DTOs;
using OnlineSurvey.Domain.Enums;

namespace OnlineSurvey.Application.Interfaces;

public interface ISurveyService
{
    Task<SurveyDetailResponse> CreateSurveyAsync(CreateSurveyRequest request, string ownerId = "", CancellationToken cancellationToken = default);
    Task<SurveyDetailResponse?> GetSurveyByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<SurveyResponse>> GetSurveysAsync(int page, int pageSize, string ownerId = "", SurveyStatus? status = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<SurveyResponse>> GetActiveSurveysAsync(CancellationToken cancellationToken = default);
    Task<SurveyDetailResponse> UpdateSurveyAsync(Guid id, UpdateSurveyRequest request, CancellationToken cancellationToken = default);
    Task<SurveyDetailResponse> ActivateSurveyAsync(Guid id, ActivateSurveyRequest request, CancellationToken cancellationToken = default);
    Task<SurveyDetailResponse> CloseSurveyAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteSurveyAsync(Guid id, CancellationToken cancellationToken = default);
}
