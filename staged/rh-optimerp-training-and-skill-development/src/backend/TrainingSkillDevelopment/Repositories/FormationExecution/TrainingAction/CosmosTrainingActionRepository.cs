using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Training.SkillDevelopment.Configuration;
using Training.SkillDevelopment.Models.Common;
using TrainingActionEntity = Training.SkillDevelopment.Models.FormationExecution.TrainingAction.TrainingAction;

namespace Training.SkillDevelopment.Repositories.FormationExecution.TrainingAction;

public sealed class CosmosTrainingActionRepository : ITrainingActionRepository
{
    private readonly Container _container;
    private readonly ILogger<CosmosTrainingActionRepository> _logger;

    public CosmosTrainingActionRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbSettings> settings,
        ILogger<CosmosTrainingActionRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(cosmosClient);
        ArgumentNullException.ThrowIfNull(settings);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var db = cosmosClient.GetDatabase(settings.Value.DatabaseName);
        _container = db.GetContainer(settings.Value.Containers["TrainingActions"]);
    }

    public async Task<TrainingActionEntity?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<TrainingActionEntity>(id, new PartitionKey(id));
            _logger.LogInformation("Action de formation {ActionId} recuperee.", id);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Action de formation {ActionId} introuvable.", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation de l'action {ActionId}.", id);
            throw;
        }
    }

    public async Task<IReadOnlyList<TrainingActionEntity>> GetAllAsync()
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.isActive = true ORDER BY c.createdAt DESC");
            return await ExecuteQueryAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation de toutes les actions de formation.");
            throw;
        }
    }

    public async Task<IReadOnlyList<TrainingActionEntity>> GetByPlanIdAsync(string planId)
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.planId = @planId AND c.isActive = true")
                .WithParameter("@planId", planId);
            return await ExecuteQueryAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation des actions pour le plan {PlanId}.", planId);
            throw;
        }
    }

    public async Task<PagedResult<TrainingActionEntity>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            var offset = (page - 1) * pageSize;
            var countQuery = new QueryDefinition("SELECT VALUE COUNT(1) FROM c WHERE c.isActive = true");
            var dataQuery = new QueryDefinition(
                "SELECT * FROM c WHERE c.isActive = true ORDER BY c.createdAt DESC OFFSET @offset LIMIT @limit")
                .WithParameter("@offset", offset)
                .WithParameter("@limit", pageSize);

            var countIterator = _container.GetItemQueryIterator<int>(countQuery);
            var countResponse = await countIterator.ReadNextAsync();
            var totalCount = countResponse.Resource.FirstOrDefault();

            var items = await ExecuteQueryAsync(dataQuery);

            return new PagedResult<TrainingActionEntity>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation paginee des actions de formation.");
            throw;
        }
    }

    public async Task<TrainingActionEntity> AddAsync(TrainingActionEntity action)
    {
        try
        {
            var response = await _container.CreateItemAsync(action, new PartitionKey(action.Id));
            _logger.LogInformation("Action de formation {ActionId} creee.", action.Id);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            _logger.LogError("Conflit : une action avec l'ID {ActionId} existe deja.", action.Id);
            throw new InvalidOperationException($"Une action de formation avec l'ID '{action.Id}' existe deja.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la creation de l'action {ActionId}.", action.Id);
            throw;
        }
    }

    public async Task<TrainingActionEntity> UpdateAsync(TrainingActionEntity action)
    {
        try
        {
            var response = await _container.UpsertItemAsync(action, new PartitionKey(action.Id));
            _logger.LogInformation("Action de formation {ActionId} mise a jour.", action.Id);
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise a jour de l'action {ActionId}.", action.Id);
            throw;
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            var action = await GetByIdAsync(id);
            if (action is null)
            {
                _logger.LogWarning("Action {ActionId} introuvable pour suppression.", id);
                return;
            }

            action.IsActive = false;
            action.DeletedAt = DateTime.UtcNow;
            await _container.UpsertItemAsync(action, new PartitionKey(id));
            _logger.LogInformation("Action de formation {ActionId} supprimee (soft-delete).", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de l'action {ActionId}.", id);
            throw;
        }
    }

    private async Task<IReadOnlyList<TrainingActionEntity>> ExecuteQueryAsync(QueryDefinition query)
    {
        var iterator = _container.GetItemQueryIterator<TrainingActionEntity>(query);
        var results = new List<TrainingActionEntity>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response.Resource);
        }

        return results.AsReadOnly();
    }
}
