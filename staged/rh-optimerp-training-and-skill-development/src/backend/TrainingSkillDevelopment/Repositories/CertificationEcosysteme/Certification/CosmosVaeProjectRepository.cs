using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Training.SkillDevelopment.Configuration;
using Training.SkillDevelopment.Models.CertificationEcosysteme.Certification;

namespace Training.SkillDevelopment.Repositories.CertificationEcosysteme.Certification;

public sealed class CosmosVaeProjectRepository : IVaeProjectRepository
{
    private readonly Container _container;
    private readonly ILogger<CosmosVaeProjectRepository> _logger;

    public CosmosVaeProjectRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbSettings> settings,
        ILogger<CosmosVaeProjectRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(cosmosClient);
        ArgumentNullException.ThrowIfNull(settings);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var db = cosmosClient.GetDatabase(settings.Value.DatabaseName);
        _container = db.GetContainer(settings.Value.Containers["VaeProjects"]);
    }

    public async Task<VaeProject?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<VaeProject>(id, new PartitionKey(id));
            _logger.LogInformation("Projet VAE {ProjectId} recupere.", id);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Projet VAE {ProjectId} introuvable.", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation du projet VAE {ProjectId}.", id);
            throw;
        }
    }

    public async Task<IReadOnlyList<VaeProject>> GetByEmployeeIdAsync(string employeeId)
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.employeeId = @employeeId ORDER BY c.startDate DESC")
                .WithParameter("@employeeId", employeeId);
            return await ExecuteQueryAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation des projets VAE pour l'employe {EmployeeId}.", employeeId);
            throw;
        }
    }

    public async Task<VaeProject> AddAsync(VaeProject project)
    {
        try
        {
            var response = await _container.CreateItemAsync(project, new PartitionKey(project.Id));
            _logger.LogInformation("Projet VAE {ProjectId} cree.", project.Id);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            _logger.LogError("Conflit : un projet VAE avec l'ID {ProjectId} existe deja.", project.Id);
            throw new InvalidOperationException($"Un projet VAE avec l'ID '{project.Id}' existe deja.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la creation du projet VAE {ProjectId}.", project.Id);
            throw;
        }
    }

    public async Task<VaeProject> UpdateAsync(VaeProject project)
    {
        try
        {
            var response = await _container.UpsertItemAsync(project, new PartitionKey(project.Id));
            _logger.LogInformation("Projet VAE {ProjectId} mis a jour.", project.Id);
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise a jour du projet VAE {ProjectId}.", project.Id);
            throw;
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            await _container.DeleteItemAsync<VaeProject>(id, new PartitionKey(id));
            _logger.LogInformation("Projet VAE {ProjectId} supprime.", id);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Projet VAE {ProjectId} introuvable pour suppression.", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du projet VAE {ProjectId}.", id);
            throw;
        }
    }

    private async Task<IReadOnlyList<VaeProject>> ExecuteQueryAsync(QueryDefinition query)
    {
        var iterator = _container.GetItemQueryIterator<VaeProject>(query);
        var results = new List<VaeProject>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response.Resource);
        }

        return results.AsReadOnly();
    }
}
