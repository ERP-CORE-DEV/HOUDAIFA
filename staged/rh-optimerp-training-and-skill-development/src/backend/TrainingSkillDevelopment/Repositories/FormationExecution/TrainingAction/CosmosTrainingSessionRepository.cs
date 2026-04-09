using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Training.SkillDevelopment.Configuration;
using Training.SkillDevelopment.Models.FormationExecution.TrainingAction;

namespace Training.SkillDevelopment.Repositories.FormationExecution.TrainingAction;

public sealed class CosmosTrainingSessionRepository : ITrainingSessionRepository
{
    private readonly Container _container;
    private readonly ILogger<CosmosTrainingSessionRepository> _logger;

    public CosmosTrainingSessionRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbSettings> settings,
        ILogger<CosmosTrainingSessionRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(cosmosClient);
        ArgumentNullException.ThrowIfNull(settings);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var db = cosmosClient.GetDatabase(settings.Value.DatabaseName);
        _container = db.GetContainer(settings.Value.Containers["TrainingSessions"]);
    }

    public async Task<TrainingSession?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<TrainingSession>(id, new PartitionKey(id));
            _logger.LogInformation("Session de formation {SessionId} recuperee.", id);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Session de formation {SessionId} introuvable.", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation de la session {SessionId}.", id);
            throw;
        }
    }

    public async Task<IReadOnlyList<TrainingSession>> GetByActionIdAsync(string actionId)
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.actionId = @actionId AND c.isActive = true")
                .WithParameter("@actionId", actionId);
            return await ExecuteQueryAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation des sessions pour l'action {ActionId}.", actionId);
            throw;
        }
    }

    public async Task<IReadOnlyList<TrainingSession>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.isActive = true AND c.startDate >= @start AND c.startDate <= @end ORDER BY c.startDate")
                .WithParameter("@start", start)
                .WithParameter("@end", end);
            return await ExecuteQueryAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation des sessions entre {Start} et {End}.", start, end);
            throw;
        }
    }

    public async Task<TrainingSession> AddAsync(TrainingSession session)
    {
        try
        {
            var response = await _container.CreateItemAsync(session, new PartitionKey(session.Id));
            _logger.LogInformation("Session de formation {SessionId} creee.", session.Id);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            _logger.LogError("Conflit : une session avec l'ID {SessionId} existe deja.", session.Id);
            throw new InvalidOperationException($"Une session de formation avec l'ID '{session.Id}' existe deja.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la creation de la session {SessionId}.", session.Id);
            throw;
        }
    }

    public async Task<TrainingSession> UpdateAsync(TrainingSession session)
    {
        try
        {
            var response = await _container.UpsertItemAsync(session, new PartitionKey(session.Id));
            _logger.LogInformation("Session de formation {SessionId} mise a jour.", session.Id);
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise a jour de la session {SessionId}.", session.Id);
            throw;
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            var session = await GetByIdAsync(id);
            if (session is null)
            {
                _logger.LogWarning("Session {SessionId} introuvable pour suppression.", id);
                return;
            }

            session.IsActive = false;
            session.UpdatedAt = DateTime.UtcNow;
            await _container.UpsertItemAsync(session, new PartitionKey(id));
            _logger.LogInformation("Session de formation {SessionId} supprimee (soft-delete).", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de la session {SessionId}.", id);
            throw;
        }
    }

    private async Task<IReadOnlyList<TrainingSession>> ExecuteQueryAsync(QueryDefinition query)
    {
        var iterator = _container.GetItemQueryIterator<TrainingSession>(query);
        var results = new List<TrainingSession>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response.Resource);
        }

        return results.AsReadOnly();
    }
}
