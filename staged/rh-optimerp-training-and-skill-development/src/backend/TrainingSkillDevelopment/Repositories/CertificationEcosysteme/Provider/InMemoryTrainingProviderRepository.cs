using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.Models.CertificationEcosysteme.Provider;

namespace Training.SkillDevelopment.Repositories.CertificationEcosysteme.Provider;

/// <summary>
/// Depot en memoire pour le developpement local sans CosmosDB.
/// NON destine a la production.
/// </summary>
public sealed class InMemoryTrainingProviderRepository : ITrainingProviderRepository
{
    private readonly ConcurrentDictionary<string, TrainingProvider> _store = new();
    private readonly ILogger<InMemoryTrainingProviderRepository> _logger;

    public InMemoryTrainingProviderRepository(ILogger<InMemoryTrainingProviderRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<TrainingProvider?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var provider);
        return Task.FromResult(provider);
    }

    public Task<IReadOnlyList<TrainingProvider>> GetAllAsync()
    {
        IReadOnlyList<TrainingProvider> result = _store.Values
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<TrainingProvider>> GetByQualiopiStatusAsync(bool hasQualiopi)
    {
        IReadOnlyList<TrainingProvider> result = _store.Values
            .Where(p => p.IsActive && p.HasQualiopiCertification == hasQualiopi)
            .OrderBy(p => p.Name)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<TrainingProvider>> SearchByNameAsync(string searchTerm)
    {
        var lower = searchTerm.ToLowerInvariant();

        IReadOnlyList<TrainingProvider> result = _store.Values
            .Where(p => p.IsActive && p.Name.ToLowerInvariant().Contains(lower))
            .OrderBy(p => p.Name)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<TrainingProvider> AddAsync(TrainingProvider provider)
    {
        if (string.IsNullOrWhiteSpace(provider.Id))
            provider.Id = Guid.NewGuid().ToString();

        provider.CreatedAt = DateTime.UtcNow;
        provider.UpdatedAt = DateTime.UtcNow;
        _store[provider.Id] = provider;
        _logger.LogInformation("Prestataire de formation {ProviderId} ajoute en memoire.", provider.Id);
        return Task.FromResult(provider);
    }

    public Task<TrainingProvider> UpdateAsync(TrainingProvider provider)
    {
        provider.UpdatedAt = DateTime.UtcNow;
        _store[provider.Id] = provider;
        return Task.FromResult(provider);
    }

    public Task DeleteAsync(string id)
    {
        if (_store.TryGetValue(id, out var provider))
        {
            provider.IsActive = false;
            provider.DeletedAt = DateTime.UtcNow;
            _store[id] = provider;
            _logger.LogInformation("Prestataire {ProviderId} supprime (soft-delete en memoire).", id);
        }

        return Task.CompletedTask;
    }
}
