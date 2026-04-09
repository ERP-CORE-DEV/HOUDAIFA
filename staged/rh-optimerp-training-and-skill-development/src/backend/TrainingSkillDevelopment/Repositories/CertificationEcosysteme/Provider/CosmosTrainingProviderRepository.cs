using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Training.SkillDevelopment.Configuration;
using Training.SkillDevelopment.Models.CertificationEcosysteme.Provider;

namespace Training.SkillDevelopment.Repositories.CertificationEcosysteme.Provider;

public sealed class CosmosTrainingProviderRepository : ITrainingProviderRepository
{
    private readonly Container _container;
    private readonly ILogger<CosmosTrainingProviderRepository> _logger;

    public CosmosTrainingProviderRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbSettings> settings,
        ILogger<CosmosTrainingProviderRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(cosmosClient);
        ArgumentNullException.ThrowIfNull(settings);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var db = cosmosClient.GetDatabase(settings.Value.DatabaseName);
        _container = db.GetContainer(settings.Value.Containers["TrainingProviders"]);
    }

    public async Task<TrainingProvider?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<TrainingProvider>(id, new PartitionKey(id));
            _logger.LogInformation("Prestataire de formation {ProviderId} recupere.", id);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Prestataire de formation {ProviderId} introuvable.", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation du prestataire {ProviderId}.", id);
            throw;
        }
    }

    public async Task<IReadOnlyList<TrainingProvider>> GetAllAsync()
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.isActive = true ORDER BY c.name");
            return await ExecuteQueryAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation de tous les prestataires.");
            throw;
        }
    }

    public async Task<IReadOnlyList<TrainingProvider>> GetByQualiopiStatusAsync(bool hasQualiopi)
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.isActive = true AND c.hasQualiopiCertification = @hasQualiopi ORDER BY c.name")
                .WithParameter("@hasQualiopi", hasQualiopi);
            return await ExecuteQueryAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation des prestataires par statut Qualiopi {HasQualiopi}.", hasQualiopi);
            throw;
        }
    }

    public async Task<IReadOnlyList<TrainingProvider>> SearchByNameAsync(string searchTerm)
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.isActive = true AND CONTAINS(LOWER(c.name), @term) ORDER BY c.name")
                .WithParameter("@term", searchTerm.ToLowerInvariant());
            return await ExecuteQueryAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recherche de prestataires par nom {SearchTerm}.", searchTerm);
            throw;
        }
    }

    public async Task<TrainingProvider> AddAsync(TrainingProvider provider)
    {
        try
        {
            var response = await _container.CreateItemAsync(provider, new PartitionKey(provider.Id));
            _logger.LogInformation("Prestataire de formation {ProviderId} cree.", provider.Id);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            _logger.LogError("Conflit : un prestataire avec l'ID {ProviderId} existe deja.", provider.Id);
            throw new InvalidOperationException($"Un prestataire de formation avec l'ID '{provider.Id}' existe deja.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la creation du prestataire {ProviderId}.", provider.Id);
            throw;
        }
    }

    public async Task<TrainingProvider> UpdateAsync(TrainingProvider provider)
    {
        try
        {
            var response = await _container.UpsertItemAsync(provider, new PartitionKey(provider.Id));
            _logger.LogInformation("Prestataire de formation {ProviderId} mis a jour.", provider.Id);
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise a jour du prestataire {ProviderId}.", provider.Id);
            throw;
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            var provider = await GetByIdAsync(id);
            if (provider is null)
            {
                _logger.LogWarning("Prestataire {ProviderId} introuvable pour suppression.", id);
                return;
            }

            provider.IsActive = false;
            provider.DeletedAt = DateTime.UtcNow;
            await _container.UpsertItemAsync(provider, new PartitionKey(id));
            _logger.LogInformation("Prestataire de formation {ProviderId} supprime (soft-delete).", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du prestataire {ProviderId}.", id);
            throw;
        }
    }

    private async Task<IReadOnlyList<TrainingProvider>> ExecuteQueryAsync(QueryDefinition query)
    {
        var iterator = _container.GetItemQueryIterator<TrainingProvider>(query);
        var results = new List<TrainingProvider>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response.Resource);
        }

        return results.AsReadOnly();
    }
}
