using Microsoft.EntityFrameworkCore;
using OnlineSurvey.Domain.Entities;
using OnlineSurvey.Domain.Repositories;
using OnlineSurvey.Infrastructure.Data;

namespace OnlineSurvey.Infrastructure.Repositories;

public class SurveyAccessCodeRepository : ISurveyAccessCodeRepository
{
    private readonly ApplicationDbContext _context;

    public SurveyAccessCodeRepository(ApplicationDbContext context) => _context = context;

    public async Task AddAsync(SurveyAccessCode code, CancellationToken cancellationToken = default)
        => await _context.AccessCodes.AddAsync(code, cancellationToken);

    public async Task<SurveyAccessCode?> GetValidCodeAsync(Guid surveyId, string email, string codeHash, CancellationToken cancellationToken = default)
        => await _context.AccessCodes
            .Where(c => c.SurveyId == surveyId
                && c.Email == email
                && c.CodeHash == codeHash
                && c.UsedAt == null
                && c.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
