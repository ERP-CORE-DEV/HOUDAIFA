using Microsoft.Azure.Cosmos;
using Training.SkillDevelopment.Models.Common;
using CompetencyModel = Training.SkillDevelopment.Models.Competency.Competency;

namespace Training.SkillDevelopment.Repositories.Competency;

public sealed class CosmosCompetencyRepository : ICompetencyRepository
{
    private readonly Container _container;

    public CosmosCompetencyRepository(Container container)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
    }

    public async Task<CompetencyModel?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        try
        {
            var response = await _container.ReadItemAsync<CompetencyModel>(
                id,
                new PartitionKey(id),
                cancellationToken: cancellationToken);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<PagedResult<CompetencyModel>> GetByDomainAsync(
        string domain, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);

        var countQuery = new QueryDefinition(
            "SELECT VALUE COUNT(1) FROM c WHERE c.domain = @domain AND c.isActive = true")
            .WithParameter("@domain", domain);

        var totalCount = await GetSingleValueAsync<int>(countQuery, cancellationToken);

        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.domain = @domain AND c.isActive = true OFFSET @offset LIMIT @limit")
            .WithParameter("@domain", domain)
            .WithParameter("@offset", (page - 1) * pageSize)
            .WithParameter("@limit", pageSize);

        var items = await ExecuteQueryAsync<CompetencyModel>(query, cancellationToken);

        return new PagedResult<CompetencyModel>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IReadOnlyList<CompetencyModel>> GetByCriticalAsync(CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.isCritical = true AND c.isActive = true");

        return await ExecuteQueryAsync<CompetencyModel>(query, cancellationToken);
    }

    public async Task<PagedResult<CompetencyModel>> SearchAsync(
        string searchTerm, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(searchTerm);

        var countQuery = new QueryDefinition(
            "SELECT VALUE COUNT(1) FROM c WHERE c.isActive = true AND (CONTAINS(LOWER(c.name), @term) OR CONTAINS(LOWER(c.code), @term))")
            .WithParameter("@term", searchTerm.ToLowerInvariant());

        var totalCount = await GetSingleValueAsync<int>(countQuery, cancellationToken);

        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.isActive = true AND (CONTAINS(LOWER(c.name), @term) OR CONTAINS(LOWER(c.code), @term)) OFFSET @offset LIMIT @limit")
            .WithParameter("@term", searchTerm.ToLowerInvariant())
            .WithParameter("@offset", (page - 1) * pageSize)
            .WithParameter("@limit", pageSize);

        var items = await ExecuteQueryAsync<CompetencyModel>(query, cancellationToken);

        return new PagedResult<CompetencyModel>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<CompetencyModel>> GetAllActiveAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var countQuery = new QueryDefinition(
            "SELECT VALUE COUNT(1) FROM c WHERE c.isActive = true");

        var totalCount = await GetSingleValueAsync<int>(countQuery, cancellationToken);

        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.isActive = true OFFSET @offset LIMIT @limit")
            .WithParameter("@offset", (page - 1) * pageSize)
            .WithParameter("@limit", pageSize);

        var items = await ExecuteQueryAsync<CompetencyModel>(query, cancellationToken);

        return new PagedResult<CompetencyModel>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CompetencyModel> CreateAsync(CompetencyModel competency, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(competency);

        var response = await _container.CreateItemAsync(
            competency,
            new PartitionKey(competency.Id),
            cancellationToken: cancellationToken);

        return response.Resource;
    }

    public async Task<CompetencyModel> UpdateAsync(CompetencyModel competency, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(competency);

        var response = await _container.ReplaceItemAsync(
            competency,
            competency.Id,
            new PartitionKey(competency.Id),
            cancellationToken: cancellationToken);

        return response.Resource;
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        await _container.DeleteItemAsync<CompetencyModel>(
            id,
            new PartitionKey(id),
            cancellationToken: cancellationToken);
    }

    private async Task<T> GetSingleValueAsync<T>(QueryDefinition query, CancellationToken cancellationToken)
    {
        using var iterator = _container.GetItemQueryIterator<T>(query);
        if (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync(cancellationToken);
            return page.FirstOrDefault()!;
        }

        return default!;
    }

    private async Task<IReadOnlyList<T>> ExecuteQueryAsync<T>(QueryDefinition query, CancellationToken cancellationToken)
    {
        var results = new List<T>();
        using var iterator = _container.GetItemQueryIterator<T>(query);

        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync(cancellationToken);
            results.AddRange(page);
        }

        return results;
    }
}
