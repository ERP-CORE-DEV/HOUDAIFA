using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FormationExecution.TrainingPlan;

namespace Training.SkillDevelopment.Repositories.FormationExecution.TrainingPlan;

/// <summary>
/// Depot en memoire pour le developpement local sans CosmosDB.
/// NON destine a la production.
/// </summary>
public sealed class InMemoryTrainingPlanRepository : ITrainingPlanRepository
{
    private readonly ConcurrentDictionary<string, Models.FormationExecution.TrainingPlan.TrainingPlan> _store = new();
    private readonly ILogger<InMemoryTrainingPlanRepository> _logger;

    public InMemoryTrainingPlanRepository(ILogger<InMemoryTrainingPlanRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Models.FormationExecution.TrainingPlan.TrainingPlan?> GetByIdAsync(string id)
        => Task.FromResult(_store.TryGetValue(id, out var plan) ? plan : null);

    public Task<IReadOnlyList<Models.FormationExecution.TrainingPlan.TrainingPlan>> GetAllAsync()
    {
        IReadOnlyList<Models.FormationExecution.TrainingPlan.TrainingPlan> result = _store.Values
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<Models.FormationExecution.TrainingPlan.TrainingPlan>> GetByYearAsync(int year)
    {
        IReadOnlyList<Models.FormationExecution.TrainingPlan.TrainingPlan> result = _store.Values
            .Where(p => p.IsActive && p.Year == year)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<Models.FormationExecution.TrainingPlan.TrainingPlan>> GetByCompanyIdAsync(string companyId)
    {
        IReadOnlyList<Models.FormationExecution.TrainingPlan.TrainingPlan> result = _store.Values
            .Where(p => p.IsActive && p.CompanyId == companyId)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<Models.FormationExecution.TrainingPlan.TrainingPlan>> GetByStatusAsync(TrainingPlanStatus status)
    {
        IReadOnlyList<Models.FormationExecution.TrainingPlan.TrainingPlan> result = _store.Values
            .Where(p => p.IsActive && p.Status == status)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<Models.FormationExecution.TrainingPlan.TrainingPlan> AddAsync(Models.FormationExecution.TrainingPlan.TrainingPlan plan)
    {
        if (string.IsNullOrWhiteSpace(plan.Id))
            plan.Id = Guid.NewGuid().ToString();

        plan.CreatedAt = DateTime.UtcNow;
        plan.UpdatedAt = DateTime.UtcNow;
        _store[plan.Id] = plan;
        _logger.LogInformation("Plan {PlanId} ajoute en memoire.", plan.Id);
        return Task.FromResult(plan);
    }

    public Task<Models.FormationExecution.TrainingPlan.TrainingPlan> UpdateAsync(Models.FormationExecution.TrainingPlan.TrainingPlan plan)
    {
        plan.UpdatedAt = DateTime.UtcNow;
        _store[plan.Id] = plan;
        return Task.FromResult(plan);
    }

    public Task<bool> DeleteAsync(string id)
    {
        if (!_store.TryGetValue(id, out var plan))
            return Task.FromResult(false);

        plan.IsActive = false;
        plan.DeletedAt = DateTime.UtcNow;
        _store[id] = plan;
        _logger.LogInformation("Plan {PlanId} supprime (soft-delete en memoire).", id);
        return Task.FromResult(true);
    }

    public Task<PagedResult<Models.FormationExecution.TrainingPlan.TrainingPlan>> GetPagedAsync(int page, int pageSize)
    {
        var active = _store.Values.Where(p => p.IsActive).OrderByDescending(p => p.CreatedAt).ToList();
        var items = active.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult(new PagedResult<Models.FormationExecution.TrainingPlan.TrainingPlan>
        {
            Items = items.AsReadOnly(),
            TotalCount = active.Count,
            Page = page,
            PageSize = pageSize
        });
    }
}
