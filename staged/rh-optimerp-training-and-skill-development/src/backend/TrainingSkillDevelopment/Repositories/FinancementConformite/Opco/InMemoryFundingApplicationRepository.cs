using System.Collections.Concurrent;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FinancementConformite.Opco;

namespace Training.SkillDevelopment.Repositories.FinancementConformite.Opco;

public sealed class InMemoryFundingApplicationRepository : IFundingApplicationRepository
{
    private readonly ConcurrentDictionary<string, FundingApplication> _store = new();

    public Task<FundingApplication?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var application);
        return Task.FromResult(application);
    }

    public Task<PagedResult<FundingApplication>> GetByOpcoIdAsync(string opcoId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var filtered = _store.Values.Where(a => a.OpcoId == opcoId).ToList();
        var items = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult(new PagedResult<FundingApplication>
        {
            Items = items,
            TotalCount = filtered.Count,
            Page = page,
            PageSize = pageSize
        });
    }

    public Task<PagedResult<FundingApplication>> GetByStatusAsync(FundingStatus status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var filtered = _store.Values.Where(a => a.Status == status).ToList();
        var items = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult(new PagedResult<FundingApplication>
        {
            Items = items,
            TotalCount = filtered.Count,
            Page = page,
            PageSize = pageSize
        });
    }

    public Task<IReadOnlyList<FundingApplication>> GetByTrainingActionIdAsync(string trainingActionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<FundingApplication> result = _store.Values
            .Where(a => a.TrainingActionId == trainingActionId)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<FundingApplication> CreateAsync(FundingApplication application, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(application);

        application.Id = application.Id == string.Empty ? Guid.NewGuid().ToString() : application.Id;
        _store[application.Id] = application;
        return Task.FromResult(application);
    }

    public Task<FundingApplication> UpdateAsync(FundingApplication application, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(application);

        _store[application.Id] = application;
        return Task.FromResult(application);
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        _store.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
