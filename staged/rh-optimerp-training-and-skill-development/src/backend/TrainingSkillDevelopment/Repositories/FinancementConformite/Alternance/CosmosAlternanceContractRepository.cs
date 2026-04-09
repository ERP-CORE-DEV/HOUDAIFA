using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Training.SkillDevelopment.Configuration;
using Training.SkillDevelopment.Models.FinancementConformite.Alternance;

namespace Training.SkillDevelopment.Repositories.FinancementConformite.Alternance;

public sealed class CosmosAlternanceContractRepository : IAlternanceContractRepository
{
    private readonly Container _container;
    private readonly ILogger<CosmosAlternanceContractRepository> _logger;

    public CosmosAlternanceContractRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbSettings> settings,
        ILogger<CosmosAlternanceContractRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(cosmosClient);
        ArgumentNullException.ThrowIfNull(settings);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var db = cosmosClient.GetDatabase(settings.Value.DatabaseName);
        _container = db.GetContainer(settings.Value.Containers["AlternanceContracts"]);
    }

    public async Task<AlternanceContract?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<AlternanceContract>(id, new PartitionKey(id));
            _logger.LogInformation("Contrat d'alternance {ContractId} recupere.", id);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Contrat d'alternance {ContractId} introuvable.", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation du contrat {ContractId}.", id);
            throw;
        }
    }

    public async Task<IReadOnlyList<AlternanceContract>> GetAllAsync()
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c ORDER BY c.createdAt DESC");
            return await ExecuteQueryAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation de tous les contrats d'alternance.");
            throw;
        }
    }

    public async Task<IReadOnlyList<AlternanceContract>> GetByEmployeeIdAsync(string employeeId)
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
            _logger.LogError(ex, "Erreur lors de la recuperation des contrats pour l'employe {EmployeeId}.", employeeId);
            throw;
        }
    }

    public async Task<IReadOnlyList<AlternanceContract>> GetActiveAsync()
    {
        try
        {
            var now = DateTime.UtcNow;
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.startDate <= @now AND c.endDate >= @now ORDER BY c.endDate")
                .WithParameter("@now", now);
            return await ExecuteQueryAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation des contrats d'alternance actifs.");
            throw;
        }
    }

    public async Task<AlternanceContract> AddAsync(AlternanceContract contract)
    {
        try
        {
            var response = await _container.CreateItemAsync(contract, new PartitionKey(contract.Id));
            _logger.LogInformation("Contrat d'alternance {ContractId} cree.", contract.Id);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            _logger.LogError("Conflit : un contrat avec l'ID {ContractId} existe deja.", contract.Id);
            throw new InvalidOperationException($"Un contrat d'alternance avec l'ID '{contract.Id}' existe deja.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la creation du contrat {ContractId}.", contract.Id);
            throw;
        }
    }

    public async Task<AlternanceContract> UpdateAsync(AlternanceContract contract)
    {
        try
        {
            var response = await _container.UpsertItemAsync(contract, new PartitionKey(contract.Id));
            _logger.LogInformation("Contrat d'alternance {ContractId} mis a jour.", contract.Id);
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise a jour du contrat {ContractId}.", contract.Id);
            throw;
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            await _container.DeleteItemAsync<AlternanceContract>(id, new PartitionKey(id));
            _logger.LogInformation("Contrat d'alternance {ContractId} supprime.", id);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Contrat {ContractId} introuvable pour suppression.", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du contrat {ContractId}.", id);
            throw;
        }
    }

    private async Task<IReadOnlyList<AlternanceContract>> ExecuteQueryAsync(QueryDefinition query)
    {
        var iterator = _container.GetItemQueryIterator<AlternanceContract>(query);
        var results = new List<AlternanceContract>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response.Resource);
        }

        return results.AsReadOnly();
    }
}
