namespace OnlineSurvey.Domain.Entities;

public class SurveyAccessCode : Entity
{
    public Guid SurveyId { get; private set; }
    public string Email { get; private set; } = null!;
    public string CodeHash { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public bool IsUsed => UsedAt.HasValue;
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    private SurveyAccessCode() { }

    public SurveyAccessCode(Guid surveyId, string email, string codeHash)
    {
        SurveyId = surveyId;
        Email = email.ToLowerInvariant();
        CodeHash = codeHash;
        ExpiresAt = DateTime.UtcNow.AddMinutes(15);
    }

    public void MarkAsUsed() => UsedAt = DateTime.UtcNow;
}
