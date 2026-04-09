using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Training.SkillDevelopment.Configuration;
using Training.SkillDevelopment.Models.FinancementConformite.Compliance;
using Training.SkillDevelopment.Models.Common;

namespace Training.SkillDevelopment.Repositories.FinancementConformite.Compliance;

public sealed class CosmosTrainingObligationRepository : ITrainingObligationRepository
{
    private readonly Container _container;
    private readonly ILogger<CosmosTrainingObligationRepository> _logger;

    public CosmosTrainingObligationRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbSettings> settings,
        ILogger<CosmosTrainingObligationRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(cosmosClient);
        ArgumentNullException.ThrowIfNull(settings);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var db = cosmosClient.GetDatabase(settings.Value.DatabaseName);
        _container = db.GetContainer(settings.Value.Containers["TrainingObligations"]);
    }

    public async Task<TrainingObligation?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<TrainingObligation>(id, new PartitionKey(id));
            _logger.LogInformation("Obligation de formation {ObligationId} recuperee.", id);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Obligation de formation {ObligationId} introuvable.", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation de l'obligation {ObligationId}.", id);
            throw;
        }
    }

    public async Task<IReadOnlyList<TrainingObligation>> GetAllAsync()
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.isActive = true ORDER BY c.title");
            return await ExecuteQueryAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation de toutes les obligations de formation.");
            throw;
        }
    }

    public async Task<IReadOnlyList<TrainingObligation>> GetActiveAsync()
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.isActive = true AND c.status = @status ORDER BY c.title")
                .WithParameter("@status", ObligationStatus.Current.ToString());
            return await ExecuteQueryAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation des obligations de formation actives.");
            throw;
        }
    }

    public async Task<IReadOnlyList<TrainingObligation>> GetByRegulatoryReferenceAsync(string regulatoryReference)
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.isActive = true AND c.regulatoryReference = @ref")
                .WithParameter("@ref", regulatoryReference);
            return await ExecuteQueryAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation des obligations par reference {Reference}.", regulatoryReference);
            throw;
        }
    }

    public async Task<PagedResult<TrainingObligation>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            var offset = (page - 1) * pageSize;
            var countQuery = new QueryDefinition("SELECT VALUE COUNT(1) FROM c WHERE c.isActive = true");
            var dataQuery = new QueryDefinition(
                "SELECT * FROM c WHERE c.isActive = true ORDER BY c.title OFFSET @offset LIMIT @limit")
                .WithParameter("@offset", offset)
                .WithParameter("@limit", pageSize);

            var countIterator = _container.GetItemQueryIterator<int>(countQuery);
            var countResponse = await countIterator.ReadNextAsync();
            var totalCount = countResponse.Resource.FirstOrDefault();

            var items = await ExecuteQueryAsync(dataQuery);

            return new PagedResult<TrainingObligation>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation paginee des obligations de formation.");
            throw;
        }
    }

    public async Task<TrainingObligation> AddAsync(TrainingObligation obligation)
    {
        try
        {
            var response = await _container.CreateItemAsync(obligation, new PartitionKey(obligation.Id));
            _logger.LogInformation("Obligation de formation {ObligationId} creee.", obligation.Id);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            _logger.LogError("Conflit : une obligation avec l'ID {ObligationId} existe deja.", obligation.Id);
            throw new InvalidOperationException($"Une obligation de formation avec l'ID '{obligation.Id}' existe deja.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la creation de l'obligation {ObligationId}.", obligation.Id);
            throw;
        }
    }

    public async Task<TrainingObligation> UpdateAsync(TrainingObligation obligation)
    {
        try
        {
            var response = await _container.UpsertItemAsync(obligation, new PartitionKey(obligation.Id));
            _logger.LogInformation("Obligation de formation {ObligationId} mise a jour.", obligation.Id);
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise a jour de l'obligation {ObligationId}.", obligation.Id);
            throw;
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            var obligation = await GetByIdAsync(id);
            if (obligation is null)
            {
                _logger.LogWarning("Obligation {ObligationId} introuvable pour suppression.", id);
                return;
            }

            obligation.IsActive = false;
            obligation.DeletedAt = DateTime.UtcNow;
            await _container.UpsertItemAsync(obligation, new PartitionKey(id));
            _logger.LogInformation("Obligation de formation {ObligationId} supprimee (soft-delete).", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de l'obligation {ObligationId}.", id);
            throw;
        }
    }

    private async Task<IReadOnlyList<TrainingObligation>> ExecuteQueryAsync(QueryDefinition query)
    {
        var iterator = _container.GetItemQueryIterator<TrainingObligation>(query);
        var results = new List<TrainingObligation>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response.Resource);
        }

        return results.AsReadOnly();
    }
}
