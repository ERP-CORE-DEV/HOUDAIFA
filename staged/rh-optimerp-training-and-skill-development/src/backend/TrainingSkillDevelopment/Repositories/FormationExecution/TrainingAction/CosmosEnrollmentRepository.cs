using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Training.SkillDevelopment.Configuration;
using Training.SkillDevelopment.Models.FormationExecution.TrainingAction;

namespace Training.SkillDevelopment.Repositories.FormationExecution.TrainingAction;

public sealed class CosmosEnrollmentRepository : IEnrollmentRepository
{
    private readonly Container _container;
    private readonly ILogger<CosmosEnrollmentRepository> _logger;

    public CosmosEnrollmentRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbSettings> settings,
        ILogger<CosmosEnrollmentRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(cosmosClient);
        ArgumentNullException.ThrowIfNull(settings);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var db = cosmosClient.GetDatabase(settings.Value.DatabaseName);
        _container = db.GetContainer(settings.Value.Containers["Enrollments"]);
    }

    public async Task<Enrollment?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<Enrollment>(id, new PartitionKey(id));
            _logger.LogInformation("Inscription {EnrollmentId} recuperee.", id);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Inscription {EnrollmentId} introuvable.", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation de l'inscription {EnrollmentId}.", id);
            throw;
        }
    }

    public async Task<IReadOnlyList<Enrollment>> GetBySessionIdAsync(string sessionId)
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
            _logger.LogError(ex, "Erreur lors de la recuperation des inscriptions pour la session {SessionId}.", sessionId);
            throw;
        }
    }

    public async Task<IReadOnlyList<Enrollment>> GetByEmployeeIdAsync(string employeeId)
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
            _logger.LogError(ex, "Erreur lors de la recuperation des inscriptions pour l'employe {EmployeeId}.", employeeId);
            throw;
        }
    }

    public async Task<Enrollment> AddAsync(Enrollment enrollment)
    {
        try
        {
            var response = await _container.CreateItemAsync(enrollment, new PartitionKey(enrollment.Id));
            _logger.LogInformation("Inscription {EnrollmentId} creee.", enrollment.Id);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            _logger.LogError("Conflit : une inscription avec l'ID {EnrollmentId} existe deja.", enrollment.Id);
            throw new InvalidOperationException($"Une inscription avec l'ID '{enrollment.Id}' existe deja.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la creation de l'inscription {EnrollmentId}.", enrollment.Id);
            throw;
        }
    }

    public async Task<Enrollment> UpdateAsync(Enrollment enrollment)
    {
        try
        {
            var response = await _container.UpsertItemAsync(enrollment, new PartitionKey(enrollment.Id));
            _logger.LogInformation("Inscription {EnrollmentId} mise a jour.", enrollment.Id);
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise a jour de l'inscription {EnrollmentId}.", enrollment.Id);
            throw;
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            await _container.DeleteItemAsync<Enrollment>(id, new PartitionKey(id));
            _logger.LogInformation("Inscription {EnrollmentId} supprimee.", id);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Inscription {EnrollmentId} introuvable pour suppression.", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de l'inscription {EnrollmentId}.", id);
            throw;
        }
    }

    private async Task<IReadOnlyList<Enrollment>> ExecuteQueryAsync(QueryDefinition query)
    {
        var iterator = _container.GetItemQueryIterator<Enrollment>(query);
        var results = new List<Enrollment>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response.Resource);
        }

        return results.AsReadOnly();
    }
}
