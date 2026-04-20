using OnlineSurvey.Domain.Entities;

namespace OnlineSurvey.Domain.Repositories;

public interface ISurveyAccessCodeRepository
{
    Task AddAsync(SurveyAccessCode code, CancellationToken cancellationToken = default);
    Task<SurveyAccessCode?> GetValidCodeAsync(Guid surveyId, string email, string codeHash, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
