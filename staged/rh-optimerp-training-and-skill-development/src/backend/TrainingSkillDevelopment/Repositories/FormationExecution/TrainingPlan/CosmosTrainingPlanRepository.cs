using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Training.SkillDevelopment.Configuration;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FormationExecution.TrainingPlan;

namespace Training.SkillDevelopment.Repositories.FormationExecution.TrainingPlan;

public sealed class CosmosTrainingPlanRepository : ITrainingPlanRepository
{
    private readonly Container _container;
    private readonly ILogger<CosmosTrainingPlanRepository> _logger;

    public CosmosTrainingPlanRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbSettings> settings,
        ILogger<CosmosTrainingPlanRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(cosmosClient);
        ArgumentNullException.ThrowIfNull(settings);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var db = cosmosClient.GetDatabase(settings.Value.DatabaseName);
        _container = db.GetContainer(settings.Value.Containers["TrainingPlans"]);
    }

    public async Task<Models.FormationExecution.TrainingPlan.TrainingPlan?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<Models.FormationExecution.TrainingPlan.TrainingPlan>(
                id, new PartitionKey(id));
            _logger.LogInformation("Plan de formation {PlanId} recupere.", id);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Plan de formation {PlanId} introuvable.", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation du plan {PlanId}.", id);
            throw;
        }
    }

    public async Task<IReadOnlyList<Models.FormationExecution.TrainingPlan.TrainingPlan>> GetAllAsync()
    {
        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.isActive = true ORDER BY c.createdAt DESC");
            return await ExecuteQueryAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation de tous les plans.");
            throw;
        }
    }

    public async Task<IReadOnlyList<Models.FormationExecution.TrainingPlan.TrainingPlan>> GetByYearAsync(int year)
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.year = @year AND c.isActive = true")
                .WithParameter("@year", year);
            return await ExecuteQueryAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation des plans pour l'annee {Year}.", year);
            throw;
        }
    }

    public async Task<IReadOnlyList<Models.FormationExecution.TrainingPlan.TrainingPlan>> GetByCompanyIdAsync(string companyId)
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.companyId = @companyId AND c.isActive = true")
                .WithParameter("@companyId", companyId);
            return await ExecuteQueryAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation des plans pour l'entreprise {CompanyId}.", companyId);
            throw;
        }
    }

    public async Task<IReadOnlyList<Models.FormationExecution.TrainingPlan.TrainingPlan>> GetByStatusAsync(TrainingPlanStatus status)
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.status = @status AND c.isActive = true")
                .WithParameter("@status", status.ToString());
            return await ExecuteQueryAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation des plans avec le statut {Status}.", status);
            throw;
        }
    }

    public async Task<Models.FormationExecution.TrainingPlan.TrainingPlan> AddAsync(Models.FormationExecution.TrainingPlan.TrainingPlan plan)
    {
        try
        {
            var response = await _container.CreateItemAsync(plan, new PartitionKey(plan.Id));
            _logger.LogInformation("Plan de formation {PlanId} cree.", plan.Id);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            _logger.LogError("Conflit : un plan avec l'ID {PlanId} existe deja.", plan.Id);
            throw new InvalidOperationException($"Un plan de formation avec l'ID '{plan.Id}' existe deja.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la creation du plan {PlanId}.", plan.Id);
            throw;
        }
    }

    public async Task<Models.FormationExecution.TrainingPlan.TrainingPlan> UpdateAsync(Models.FormationExecution.TrainingPlan.TrainingPlan plan)
    {
        try
        {
            var response = await _container.UpsertItemAsync(plan, new PartitionKey(plan.Id));
            _logger.LogInformation("Plan de formation {PlanId} mis a jour.", plan.Id);
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise a jour du plan {PlanId}.", plan.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            var plan = await GetByIdAsync(id);
            if (plan is null)
            {
                _logger.LogWarning("Plan {PlanId} introuvable pour suppression.", id);
                return false;
            }

            plan.IsActive = false;
            plan.DeletedAt = DateTime.UtcNow;
            await _container.UpsertItemAsync(plan, new PartitionKey(id));
            _logger.LogInformation("Plan de formation {PlanId} supprime (soft-delete).", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du plan {PlanId}.", id);
            throw;
        }
    }

    public async Task<PagedResult<Models.FormationExecution.TrainingPlan.TrainingPlan>> GetPagedAsync(int page, int pageSize)
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

            return new PagedResult<Models.FormationExecution.TrainingPlan.TrainingPlan>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation paginee des plans.");
            throw;
        }
    }

    private async Task<IReadOnlyList<Models.FormationExecution.TrainingPlan.TrainingPlan>> ExecuteQueryAsync(QueryDefinition query)
    {
        var iterator = _container.GetItemQueryIterator<Models.FormationExecution.TrainingPlan.TrainingPlan>(query);
        var results = new List<Models.FormationExecution.TrainingPlan.TrainingPlan>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response.Resource);
        }

        return results.AsReadOnly();
    }
}
