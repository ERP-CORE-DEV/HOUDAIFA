using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.Models.Common;
using TrainingActionEntity = Training.SkillDevelopment.Models.FormationExecution.TrainingAction.TrainingAction;

namespace Training.SkillDevelopment.Repositories.FormationExecution.TrainingAction;

/// <summary>
/// Depot en memoire pour le developpement local sans CosmosDB.
/// NON destine a la production.
/// </summary>
public sealed class InMemoryTrainingActionRepository : ITrainingActionRepository
{
    private readonly ConcurrentDictionary<string, TrainingActionEntity> _store = new();
    private readonly ILogger<InMemoryTrainingActionRepository> _logger;

    public InMemoryTrainingActionRepository(ILogger<InMemoryTrainingActionRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<TrainingActionEntity?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var action);
        return Task.FromResult(action);
    }

    public Task<IReadOnlyList<TrainingActionEntity>> GetAllAsync()
    {
        IReadOnlyList<TrainingActionEntity> result = _store.Values
            .Where(a => a.IsActive)
            .OrderByDescending(a => a.CreatedAt)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<TrainingActionEntity>> GetByPlanIdAsync(string planId)
    {
        IReadOnlyList<TrainingActionEntity> result = _store.Values
            .Where(a => a.IsActive && a.PlanId == planId)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<PagedResult<TrainingActionEntity>> GetPagedAsync(int page, int pageSize)
    {
        var active = _store.Values
            .Where(a => a.IsActive)
            .OrderByDescending(a => a.CreatedAt)
            .ToList();

        var items = active.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult(new PagedResult<TrainingActionEntity>
        {
            Items = items.AsReadOnly(),
            TotalCount = active.Count,
            Page = page,
            PageSize = pageSize
        });
    }

    public Task<TrainingActionEntity> AddAsync(TrainingActionEntity action)
    {
        if (string.IsNullOrWhiteSpace(action.Id))
            action.Id = Guid.NewGuid().ToString();

        action.CreatedAt = DateTime.UtcNow;
        action.UpdatedAt = DateTime.UtcNow;
        _store[action.Id] = action;
        _logger.LogInformation("Action de formation {ActionId} ajoutee en memoire.", action.Id);
        return Task.FromResult(action);
    }

    public Task<TrainingActionEntity> UpdateAsync(TrainingActionEntity action)
    {
        action.UpdatedAt = DateTime.UtcNow;
        _store[action.Id] = action;
        return Task.FromResult(action);
    }

    public Task DeleteAsync(string id)
    {
        if (_store.TryGetValue(id, out var action))
        {
            action.IsActive = false;
            action.DeletedAt = DateTime.UtcNow;
            _store[id] = action;
            _logger.LogInformation("Action de formation {ActionId} supprimee (soft-delete en memoire).", id);
        }

        return Task.CompletedTask;
    }
}
