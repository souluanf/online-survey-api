namespace OnlineSurvey.Application.Interfaces;

public interface IEmailService
{
    Task SendAccessCodeAsync(string to, string surveyTitle, string code, CancellationToken cancellationToken = default);
}
