using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.Models.FinancementConformite.Compliance;
using Training.SkillDevelopment.Models.Common;

namespace Training.SkillDevelopment.Repositories.FinancementConformite.Compliance;

/// <summary>
/// Depot en memoire pour le developpement local sans CosmosDB.
/// NON destine a la production.
/// </summary>
public sealed class InMemoryTrainingObligationRepository : ITrainingObligationRepository
{
    private readonly ConcurrentDictionary<string, TrainingObligation> _store = new();
    private readonly ILogger<InMemoryTrainingObligationRepository> _logger;

    public InMemoryTrainingObligationRepository(ILogger<InMemoryTrainingObligationRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<TrainingObligation?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var obligation);
        return Task.FromResult(obligation);
    }

    public Task<IReadOnlyList<TrainingObligation>> GetAllAsync()
    {
        IReadOnlyList<TrainingObligation> result = _store.Values
            .Where(o => o.IsActive)
            .OrderBy(o => o.Title)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<TrainingObligation>> GetActiveAsync()
    {
        IReadOnlyList<TrainingObligation> result = _store.Values
            .Where(o => o.IsActive && o.Status == ObligationStatus.Current)
            .OrderBy(o => o.Title)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<TrainingObligation>> GetByRegulatoryReferenceAsync(string regulatoryReference)
    {
        IReadOnlyList<TrainingObligation> result = _store.Values
            .Where(o => o.IsActive && o.RegulatoryReference == regulatoryReference)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<PagedResult<TrainingObligation>> GetPagedAsync(int page, int pageSize)
    {
        var active = _store.Values.Where(o => o.IsActive).OrderBy(o => o.Title).ToList();
        var items = active.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult(new PagedResult<TrainingObligation>
        {
            Items = items.AsReadOnly(),
            TotalCount = active.Count,
            Page = page,
            PageSize = pageSize
        });
    }

    public Task<TrainingObligation> AddAsync(TrainingObligation obligation)
    {
        if (string.IsNullOrWhiteSpace(obligation.Id))
            obligation.Id = Guid.NewGuid().ToString();

        obligation.CreatedAt = DateTime.UtcNow;
        obligation.UpdatedAt = DateTime.UtcNow;
        _store[obligation.Id] = obligation;
        _logger.LogInformation("Obligation de formation {ObligationId} ajoutee en memoire.", obligation.Id);
        return Task.FromResult(obligation);
    }

    public Task<TrainingObligation> UpdateAsync(TrainingObligation obligation)
    {
        obligation.UpdatedAt = DateTime.UtcNow;
        _store[obligation.Id] = obligation;
        return Task.FromResult(obligation);
    }

    public Task DeleteAsync(string id)
    {
        if (_store.TryGetValue(id, out var obligation))
        {
            obligation.IsActive = false;
            obligation.DeletedAt = DateTime.UtcNow;
            _store[id] = obligation;
            _logger.LogInformation("Obligation de formation {ObligationId} supprimee (soft-delete en memoire).", id);
        }

        return Task.CompletedTask;
    }
}
