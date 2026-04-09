using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.Models.CertificationEcosysteme.Certification;

namespace Training.SkillDevelopment.Repositories.CertificationEcosysteme.Certification;

/// <summary>
/// Depot en memoire pour le developpement local sans CosmosDB.
/// NON destine a la production.
/// </summary>
public sealed class InMemoryVaeProjectRepository : IVaeProjectRepository
{
    private readonly ConcurrentDictionary<string, VaeProject> _store = new();
    private readonly ILogger<InMemoryVaeProjectRepository> _logger;

    public InMemoryVaeProjectRepository(ILogger<InMemoryVaeProjectRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<VaeProject?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var project);
        return Task.FromResult(project);
    }

    public Task<IReadOnlyList<VaeProject>> GetByEmployeeIdAsync(string employeeId)
    {
        IReadOnlyList<VaeProject> result = _store.Values
            .Where(p => p.EmployeeId == employeeId)
            .OrderByDescending(p => p.StartDate)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<VaeProject> AddAsync(VaeProject project)
    {
        if (string.IsNullOrWhiteSpace(project.Id))
            project.Id = Guid.NewGuid().ToString();

        project.CreatedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;
        _store[project.Id] = project;
        _logger.LogInformation("Projet VAE {ProjectId} ajoute en memoire.", project.Id);
        return Task.FromResult(project);
    }

    public Task<VaeProject> UpdateAsync(VaeProject project)
    {
        project.UpdatedAt = DateTime.UtcNow;
        _store[project.Id] = project;
        return Task.FromResult(project);
    }

    public Task DeleteAsync(string id)
    {
        _store.TryRemove(id, out _);
        _logger.LogInformation("Projet VAE {ProjectId} supprime en memoire.", id);
        return Task.CompletedTask;
    }
}
