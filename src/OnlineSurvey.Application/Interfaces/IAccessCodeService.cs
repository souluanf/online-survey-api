namespace OnlineSurvey.Application.Interfaces;

public interface IAccessCodeService
{
    Task<bool> RequestCodeAsync(Guid surveyId, string email, CancellationToken cancellationToken = default);
    Task<bool> VerifyCodeAsync(Guid surveyId, string email, string code, CancellationToken cancellationToken = default);
}
