using Microsoft.EntityFrameworkCore;
using OnlineSurvey.Domain.Entities;
using OnlineSurvey.Domain.Enums;
using OnlineSurvey.Domain.Repositories;
using OnlineSurvey.Infrastructure.Data;

namespace OnlineSurvey.Infrastructure.Repositories;

public class SurveyRepository : Repository<Survey>, ISurveyRepository
{
    public SurveyRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Survey?> GetByIdWithQuestionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Questions.OrderBy(q => q.Order))
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Survey?> GetByIdWithQuestionsAndOptionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Questions.OrderBy(q => q.Order))
                .ThenInclude(q => q.Options.OrderBy(o => o.Order))
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Survey>> GetByStatusAsync(SurveyStatus status, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => s.Status == status)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Survey>> GetActiveSurveysAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Where(s => s.Status == SurveyStatus.Active)
            .Where(s => s.EndDate == null || s.EndDate > now)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Survey> Surveys, int TotalCount)> GetPaginatedAsync(
        int page,
        int pageSize,
        SurveyStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var surveys = await query
            .Include(s => s.Questions)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (surveys, totalCount);
    }

    public async Task<(IEnumerable<Survey> Surveys, int TotalCount)> GetPaginatedByOwnerAsync(
        string ownerId,
        int page,
        int pageSize,
        SurveyStatus? status,
        CancellationToken cancellationToken)
    {
        var query = DbSet.Where(s => s.OwnerId == ownerId);

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var surveys = await query
            .Include(s => s.Questions)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (surveys, totalCount);
    }
}
