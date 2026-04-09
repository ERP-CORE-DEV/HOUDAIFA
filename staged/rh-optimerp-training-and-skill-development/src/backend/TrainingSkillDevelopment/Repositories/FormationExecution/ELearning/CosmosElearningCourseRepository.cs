using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Training.SkillDevelopment.Configuration;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FormationExecution.ELearning;

namespace Training.SkillDevelopment.Repositories.FormationExecution.ELearning;

public sealed class CosmosElearningCourseRepository : IElearningCourseRepository
{
    private readonly Container _container;
    private readonly ILogger<CosmosElearningCourseRepository> _logger;

    public CosmosElearningCourseRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbSettings> settings,
        ILogger<CosmosElearningCourseRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(cosmosClient);
        ArgumentNullException.ThrowIfNull(settings);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var db = cosmosClient.GetDatabase(settings.Value.DatabaseName);
        _container = db.GetContainer(settings.Value.Containers["ElearningCourses"]);
    }

    public async Task<ElearningCourse?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<ElearningCourse>(id, new PartitionKey(id));
            _logger.LogInformation("Cours e-learning {CourseId} recupere.", id);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Cours e-learning {CourseId} introuvable.", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation du cours {CourseId}.", id);
            throw;
        }
    }

    public async Task<IReadOnlyList<ElearningCourse>> GetAllAsync()
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.isActive = true ORDER BY c.createdAt DESC");
            return await ExecuteQueryAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation de tous les cours e-learning.");
            throw;
        }
    }

    public async Task<IReadOnlyList<ElearningCourse>> GetByTagAsync(string tag)
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.isActive = true AND ARRAY_CONTAINS(c.tags, @tag)")
                .WithParameter("@tag", tag);
            return await ExecuteQueryAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recherche des cours par tag {Tag}.", tag);
            throw;
        }
    }

    public async Task<IReadOnlyList<ElearningCourse>> GetByCompetencyIdAsync(string competencyId)
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.isActive = true AND ARRAY_CONTAINS(c.competencyIds, @competencyId)")
                .WithParameter("@competencyId", competencyId);
            return await ExecuteQueryAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recherche des cours par competence {CompetencyId}.", competencyId);
            throw;
        }
    }

    public async Task<ElearningCourse> AddAsync(ElearningCourse course)
    {
        try
        {
            var response = await _container.CreateItemAsync(course, new PartitionKey(course.Id));
            _logger.LogInformation("Cours e-learning {CourseId} cree.", course.Id);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            _logger.LogError("Conflit : un cours avec l'ID {CourseId} existe deja.", course.Id);
            throw new InvalidOperationException($"Un cours e-learning avec l'ID '{course.Id}' existe deja.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la creation du cours {CourseId}.", course.Id);
            throw;
        }
    }

    public async Task<ElearningCourse> UpdateAsync(ElearningCourse course)
    {
        try
        {
            var response = await _container.UpsertItemAsync(course, new PartitionKey(course.Id));
            _logger.LogInformation("Cours e-learning {CourseId} mis a jour.", course.Id);
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise a jour du cours {CourseId}.", course.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            var course = await GetByIdAsync(id);
            if (course is null)
            {
                _logger.LogWarning("Cours {CourseId} introuvable pour suppression.", id);
                return false;
            }

            course.IsActive = false;
            course.UpdatedAt = DateTime.UtcNow;
            await _container.UpsertItemAsync(course, new PartitionKey(id));
            _logger.LogInformation("Cours e-learning {CourseId} supprime (soft-delete).", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du cours {CourseId}.", id);
            throw;
        }
    }

    public async Task<PagedResult<ElearningCourse>> GetPagedAsync(int page, int pageSize)
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

            return new PagedResult<ElearningCourse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation paginee des cours e-learning.");
            throw;
        }
    }

    private async Task<IReadOnlyList<ElearningCourse>> ExecuteQueryAsync(QueryDefinition query)
    {
        var iterator = _container.GetItemQueryIterator<ElearningCourse>(query);
        var results = new List<ElearningCourse>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response.Resource);
        }

        return results.AsReadOnly();
    }
}
