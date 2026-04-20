using OnlineSurvey.Application.Interfaces;
using OnlineSurvey.Domain.Entities;
using OnlineSurvey.Domain.Repositories;

namespace OnlineSurvey.Application.Services;

public class AccessCodeService : IAccessCodeService
{
    private readonly ISurveyAccessCodeRepository _codeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public AccessCodeService(ISurveyAccessCodeRepository codeRepository, IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _codeRepository = codeRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<bool> RequestCodeAsync(Guid surveyId, string email, CancellationToken cancellationToken = default)
    {
        var survey = await _unitOfWork.Surveys.GetByIdAsync(surveyId, cancellationToken);
        if (survey is null || !survey.IsOpen) return false;

        var code = GenerateCode();
        var hash = HashCode(code);
        var accessCode = new SurveyAccessCode(surveyId, email, hash);

        await _codeRepository.AddAsync(accessCode, cancellationToken);
        await _codeRepository.SaveChangesAsync(cancellationToken);
        await _emailService.SendAccessCodeAsync(email, survey.Title, code, cancellationToken);

        return true;
    }

    public async Task<bool> VerifyCodeAsync(Guid surveyId, string email, string code, CancellationToken cancellationToken = default)
    {
        var hash = HashCode(code);
        var accessCode = await _codeRepository.GetValidCodeAsync(surveyId, email.ToLowerInvariant(), hash, cancellationToken);
        if (accessCode is null) return false;

        accessCode.MarkAsUsed();
        await _codeRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string GenerateCode() => Random.Shared.Next(100000, 999999).ToString();

    private static string HashCode(string code)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(bytes);
    }
}
