using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.Models.CertificationEcosysteme.Certification;

namespace Training.SkillDevelopment.Repositories.CertificationEcosysteme.Certification;

/// <summary>
/// Depot en memoire pour le developpement local sans CosmosDB.
/// NON destine a la production.
/// </summary>
public sealed class InMemoryCertificationRepository : ICertificationRepository
{
    private readonly ConcurrentDictionary<string, CertificationRncp> _store = new();
    private readonly ILogger<InMemoryCertificationRepository> _logger;

    public InMemoryCertificationRepository(ILogger<InMemoryCertificationRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<CertificationRncp?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var cert);
        return Task.FromResult(cert);
    }

    public Task<IReadOnlyList<CertificationRncp>> GetAllAsync()
    {
        IReadOnlyList<CertificationRncp> result = _store.Values
            .Where(c => c.IsActive)
            .OrderBy(c => c.Title)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<CertificationRncp?> GetByRncpCodeAsync(string rncpCode)
    {
        var cert = _store.Values.FirstOrDefault(c => c.IsActive && c.RncpCode == rncpCode);
        return Task.FromResult(cert);
    }

    public Task<CertificationRncp> AddAsync(CertificationRncp cert)
    {
        if (string.IsNullOrWhiteSpace(cert.Id))
            cert.Id = Guid.NewGuid().ToString();

        cert.CreatedAt = DateTime.UtcNow;
        cert.UpdatedAt = DateTime.UtcNow;
        _store[cert.Id] = cert;
        _logger.LogInformation("Certification RNCP {CertId} ajoutee en memoire.", cert.Id);
        return Task.FromResult(cert);
    }

    public Task<CertificationRncp> UpdateAsync(CertificationRncp cert)
    {
        cert.UpdatedAt = DateTime.UtcNow;
        _store[cert.Id] = cert;
        return Task.FromResult(cert);
    }

    public Task DeleteAsync(string id)
    {
        if (_store.TryGetValue(id, out var cert))
        {
            cert.IsActive = false;
            cert.DeletedAt = DateTime.UtcNow;
            _store[id] = cert;
            _logger.LogInformation("Certification RNCP {CertId} supprimee (soft-delete en memoire).", id);
        }

        return Task.CompletedTask;
    }
}
