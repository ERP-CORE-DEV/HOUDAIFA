using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Training.SkillDevelopment.Configuration;
using Training.SkillDevelopment.Models.FormationExecution.Evaluation;

namespace Training.SkillDevelopment.Repositories.FormationExecution.Evaluation;

public sealed class CosmosTrainingEvaluationRepository : ITrainingEvaluationRepository
{
    private readonly Container _container;
    private readonly ILogger<CosmosTrainingEvaluationRepository> _logger;

    public CosmosTrainingEvaluationRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbSettings> settings,
        ILogger<CosmosTrainingEvaluationRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(cosmosClient);
        ArgumentNullException.ThrowIfNull(settings);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var db = cosmosClient.GetDatabase(settings.Value.DatabaseName);
        _container = db.GetContainer(settings.Value.Containers["TrainingEvaluations"]);
    }

    public async Task<TrainingEvaluation?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<TrainingEvaluation>(id, new PartitionKey(id));
            _logger.LogInformation("Evaluation de formation {EvaluationId} recuperee.", id);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Evaluation de formation {EvaluationId} introuvable.", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation de l'evaluation {EvaluationId}.", id);
            throw;
        }
    }

    public async Task<IReadOnlyList<TrainingEvaluation>> GetBySessionIdAsync(string sessionId)
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.sessionId = @sessionId")
                .WithParameter("@sessionId", sessionId);
            return await ExecuteQueryAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation des evaluations pour la session {SessionId}.", sessionId);
            throw;
        }
    }

    public async Task<IReadOnlyList<TrainingEvaluation>> GetByEmployeeIdAsync(string employeeId)
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.employeeId = @employeeId")
                .WithParameter("@employeeId", employeeId);
            return await ExecuteQueryAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation des evaluations pour l'employe {EmployeeId}.", employeeId);
            throw;
        }
    }

    public async Task<decimal> GetAverageScoreAsync(string sessionId)
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT VALUE AVG(c.score) FROM c WHERE c.sessionId = @sessionId")
                .WithParameter("@sessionId", sessionId);

            using var iterator = _container.GetItemQueryIterator<decimal?>(query);

            if (iterator.HasMoreResults)
            {
                var page = await iterator.ReadNextAsync();
                var average = page.FirstOrDefault();
                return average.HasValue ? Math.Round(average.Value, 2) : 0m;
            }

            return 0m;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du calcul de la moyenne pour la session {SessionId}.", sessionId);
            throw;
        }
    }

    public async Task<TrainingEvaluation> AddAsync(TrainingEvaluation evaluation)
    {
        try
        {
            var response = await _container.CreateItemAsync(evaluation, new PartitionKey(evaluation.Id));
            _logger.LogInformation("Evaluation de formation {EvaluationId} creee.", evaluation.Id);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            _logger.LogError("Conflit : une evaluation avec l'ID {EvaluationId} existe deja.", evaluation.Id);
            throw new InvalidOperationException($"Une evaluation avec l'ID '{evaluation.Id}' existe deja.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la creation de l'evaluation {EvaluationId}.", evaluation.Id);
            throw;
        }
    }

    public async Task<TrainingEvaluation> UpdateAsync(TrainingEvaluation evaluation)
    {
        try
        {
            var response = await _container.UpsertItemAsync(evaluation, new PartitionKey(evaluation.Id));
            _logger.LogInformation("Evaluation de formation {EvaluationId} mise a jour.", evaluation.Id);
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise a jour de l'evaluation {EvaluationId}.", evaluation.Id);
            throw;
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            await _container.DeleteItemAsync<TrainingEvaluation>(id, new PartitionKey(id));
            _logger.LogInformation("Evaluation de formation {EvaluationId} supprimee.", id);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Evaluation {EvaluationId} introuvable pour suppression.", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de l'evaluation {EvaluationId}.", id);
            throw;
        }
    }

    private async Task<IReadOnlyList<TrainingEvaluation>> ExecuteQueryAsync(QueryDefinition query)
    {
        var iterator = _container.GetItemQueryIterator<TrainingEvaluation>(query);
        var results = new List<TrainingEvaluation>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response.Resource);
        }

        return results.AsReadOnly();
    }
}
