using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Training.SkillDevelopment.Configuration;
using Training.SkillDevelopment.Models.CertificationEcosysteme.Certification;

namespace Training.SkillDevelopment.Repositories.CertificationEcosysteme.Certification;

public sealed class CosmosCertificationRepository : ICertificationRepository
{
    private readonly Container _container;
    private readonly ILogger<CosmosCertificationRepository> _logger;

    public CosmosCertificationRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbSettings> settings,
        ILogger<CosmosCertificationRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(cosmosClient);
        ArgumentNullException.ThrowIfNull(settings);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var db = cosmosClient.GetDatabase(settings.Value.DatabaseName);
        _container = db.GetContainer(settings.Value.Containers["Certifications"]);
    }

    public async Task<CertificationRncp?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<CertificationRncp>(id, new PartitionKey(id));
            _logger.LogInformation("Certification RNCP {CertId} recuperee.", id);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Certification RNCP {CertId} introuvable.", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation de la certification {CertId}.", id);
            throw;
        }
    }

    public async Task<IReadOnlyList<CertificationRncp>> GetAllAsync()
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.isActive = true ORDER BY c.title");
            return await ExecuteQueryAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation de toutes les certifications.");
            throw;
        }
    }

    public async Task<CertificationRncp?> GetByRncpCodeAsync(string rncpCode)
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.rncpCode = @rncpCode AND c.isActive = true")
                .WithParameter("@rncpCode", rncpCode);

            using var iterator = _container.GetItemQueryIterator<CertificationRncp>(query);

            while (iterator.HasMoreResults)
            {
                var page = await iterator.ReadNextAsync();
                var item = page.FirstOrDefault();
                if (item is not null)
                    return item;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recherche par code RNCP {RncpCode}.", rncpCode);
            throw;
        }
    }

    public async Task<CertificationRncp> AddAsync(CertificationRncp cert)
    {
        try
        {
            var response = await _container.CreateItemAsync(cert, new PartitionKey(cert.Id));
            _logger.LogInformation("Certification RNCP {CertId} creee.", cert.Id);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            _logger.LogError("Conflit : une certification avec l'ID {CertId} existe deja.", cert.Id);
            throw new InvalidOperationException($"Une certification avec l'ID '{cert.Id}' existe deja.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la creation de la certification {CertId}.", cert.Id);
            throw;
        }
    }

    public async Task<CertificationRncp> UpdateAsync(CertificationRncp cert)
    {
        try
        {
            var response = await _container.UpsertItemAsync(cert, new PartitionKey(cert.Id));
            _logger.LogInformation("Certification RNCP {CertId} mise a jour.", cert.Id);
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise a jour de la certification {CertId}.", cert.Id);
            throw;
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            var cert = await GetByIdAsync(id);
            if (cert is null)
            {
                _logger.LogWarning("Certification {CertId} introuvable pour suppression.", id);
                return;
            }

            cert.IsActive = false;
            cert.DeletedAt = DateTime.UtcNow;
            await _container.UpsertItemAsync(cert, new PartitionKey(id));
            _logger.LogInformation("Certification RNCP {CertId} supprimee (soft-delete).", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de la certification {CertId}.", id);
            throw;
        }
    }

    private async Task<IReadOnlyList<CertificationRncp>> ExecuteQueryAsync(QueryDefinition query)
    {
        var iterator = _container.GetItemQueryIterator<CertificationRncp>(query);
        var results = new List<CertificationRncp>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response.Resource);
        }

        return results.AsReadOnly();
    }
}
