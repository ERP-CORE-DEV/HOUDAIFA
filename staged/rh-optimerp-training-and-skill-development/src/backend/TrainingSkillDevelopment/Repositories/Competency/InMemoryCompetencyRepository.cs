using System.Collections.Concurrent;
using Training.SkillDevelopment.Models.Common;
using CompetencyModel = Training.SkillDevelopment.Models.Competency.Competency;

namespace Training.SkillDevelopment.Repositories.Competency;

public sealed class InMemoryCompetencyRepository : ICompetencyRepository
{
    private readonly ConcurrentDictionary<string, CompetencyModel> _store = new();

    public Task<CompetencyModel?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var item);
        return Task.FromResult(item);
    }

    public Task<PagedResult<CompetencyModel>> GetByDomainAsync(
        string domain, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var filtered = _store.Values
            .Where(c => c.Domain == domain && c.IsActive)
            .ToList();

        var paged = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult(new PagedResult<CompetencyModel>
        {
            Items = paged,
            TotalCount = filtered.Count,
            Page = page,
            PageSize = pageSize
        });
    }

    public Task<IReadOnlyList<CompetencyModel>> GetByCriticalAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CompetencyModel> result = _store.Values
            .Where(c => c.IsCritical && c.IsActive)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<PagedResult<CompetencyModel>> SearchAsync(
        string searchTerm, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var lower = searchTerm.ToLowerInvariant();
        var filtered = _store.Values
            .Where(c => c.IsActive &&
                        (c.Name.ToLowerInvariant().Contains(lower) ||
                         c.Code.ToLowerInvariant().Contains(lower)))
            .ToList();

        var paged = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult(new PagedResult<CompetencyModel>
        {
            Items = paged,
            TotalCount = filtered.Count,
            Page = page,
            PageSize = pageSize
        });
    }

    public Task<PagedResult<CompetencyModel>> GetAllActiveAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var filtered = _store.Values.Where(c => c.IsActive).ToList();
        var paged = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult(new PagedResult<CompetencyModel>
        {
            Items = paged,
            TotalCount = filtered.Count,
            Page = page,
            PageSize = pageSize
        });
    }

    public Task<CompetencyModel> CreateAsync(CompetencyModel competency, CancellationToken cancellationToken = default)
    {
        _store[competency.Id] = competency;
        return Task.FromResult(competency);
    }

    public Task<CompetencyModel> UpdateAsync(CompetencyModel competency, CancellationToken cancellationToken = default)
    {
        _store[competency.Id] = competency;
        return Task.FromResult(competency);
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        _store.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
