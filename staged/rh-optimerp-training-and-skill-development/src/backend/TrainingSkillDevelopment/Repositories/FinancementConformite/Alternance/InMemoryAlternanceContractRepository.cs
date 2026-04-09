using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.Models.FinancementConformite.Alternance;

namespace Training.SkillDevelopment.Repositories.FinancementConformite.Alternance;

/// <summary>
/// Depot en memoire pour le developpement local sans CosmosDB.
/// NON destine a la production.
/// </summary>
public sealed class InMemoryAlternanceContractRepository : IAlternanceContractRepository
{
    private readonly ConcurrentDictionary<string, AlternanceContract> _store = new();
    private readonly ILogger<InMemoryAlternanceContractRepository> _logger;

    public InMemoryAlternanceContractRepository(ILogger<InMemoryAlternanceContractRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<AlternanceContract?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var contract);
        return Task.FromResult(contract);
    }

    public Task<IReadOnlyList<AlternanceContract>> GetAllAsync()
    {
        IReadOnlyList<AlternanceContract> result = _store.Values
            .OrderByDescending(c => c.CreatedAt)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<AlternanceContract>> GetByEmployeeIdAsync(string employeeId)
    {
        IReadOnlyList<AlternanceContract> result = _store.Values
            .Where(c => c.EmployeeId == employeeId)
            .OrderByDescending(c => c.StartDate)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<AlternanceContract>> GetActiveAsync()
    {
        var now = DateTime.UtcNow;

        IReadOnlyList<AlternanceContract> result = _store.Values
            .Where(c => c.StartDate <= now && c.EndDate >= now)
            .OrderBy(c => c.EndDate)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<AlternanceContract> AddAsync(AlternanceContract contract)
    {
        if (string.IsNullOrWhiteSpace(contract.Id))
            contract.Id = Guid.NewGuid().ToString();

        contract.CreatedAt = DateTime.UtcNow;
        contract.UpdatedAt = DateTime.UtcNow;
        _store[contract.Id] = contract;
        _logger.LogInformation("Contrat d'alternance {ContractId} ajoute en memoire.", contract.Id);
        return Task.FromResult(contract);
    }

    public Task<AlternanceContract> UpdateAsync(AlternanceContract contract)
    {
        contract.UpdatedAt = DateTime.UtcNow;
        _store[contract.Id] = contract;
        return Task.FromResult(contract);
    }

    public Task DeleteAsync(string id)
    {
        _store.TryRemove(id, out _);
        _logger.LogInformation("Contrat d'alternance {ContractId} supprime en memoire.", id);
        return Task.CompletedTask;
    }
}
