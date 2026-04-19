using OnlineSurvey.Domain.Entities;
using OnlineSurvey.Domain.Enums;

namespace OnlineSurvey.Domain.Repositories;

public interface ISurveyRepository : IRepository<Survey>
{
    Task<Survey?> GetByIdWithQuestionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Survey?> GetByIdWithQuestionsAndOptionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Survey>> GetByStatusAsync(SurveyStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Survey>> GetActiveSurveysAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<Survey> Surveys, int TotalCount)> GetPaginatedAsync(
        int page,
        int pageSize,
        SurveyStatus? status = null,
        CancellationToken cancellationToken = default);
    Task<(IEnumerable<Survey> Surveys, int TotalCount)> GetPaginatedByOwnerAsync(
        string ownerId,
        int page,
        int pageSize,
        SurveyStatus? status,
        CancellationToken cancellationToken);
}
