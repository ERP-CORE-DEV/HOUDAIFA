using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Training.SkillDevelopment.Configuration;
using Training.SkillDevelopment.Models.Common;

namespace Training.SkillDevelopment.Repositories.FinancementConformite.Opco;

public sealed class CosmosOpcoRepository : IOpcoRepository
{
    private readonly Container _container;

    public CosmosOpcoRepository(CosmosClient cosmosClient, IOptions<CosmosDbSettings> settings)
    {
        ArgumentNullException.ThrowIfNull(cosmosClient);
        ArgumentNullException.ThrowIfNull(settings);

        var dbSettings = settings.Value;
        _container = cosmosClient.GetContainer(
            dbSettings.DatabaseName,
            dbSettings.Containers["Opcos"]);
    }

    public async Task<Models.FinancementConformite.Opco.Opco?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<Models.FinancementConformite.Opco.Opco>(
                id, new PartitionKey(id), cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Models.FinancementConformite.Opco.Opco?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        const string queryText = "SELECT * FROM c WHERE c.Code = @code";
        var queryDefinition = new QueryDefinition(queryText)
            .WithParameter("@code", code);

        using var iterator = _container.GetItemQueryIterator<Models.FinancementConformite.Opco.Opco>(queryDefinition);

        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync(cancellationToken);
            var item = page.FirstOrDefault();
            if (item is not null)
                return item;
        }

        return null;
    }

    public async Task<PagedResult<Models.FinancementConformite.Opco.Opco>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var countQuery = new QueryDefinition("SELECT VALUE COUNT(1) FROM c");
        var countIterator = _container.GetItemQueryIterator<int>(countQuery);
        var countPage = await countIterator.ReadNextAsync(cancellationToken);
        var totalCount = countPage.FirstOrDefault();

        var offset = (page - 1) * pageSize;
        var dataQuery = new QueryDefinition(
            "SELECT * FROM c ORDER BY c._ts DESC OFFSET @offset LIMIT @limit")
            .WithParameter("@offset", offset)
            .WithParameter("@limit", pageSize);

        var items = new List<Models.FinancementConformite.Opco.Opco>();
        using var dataIterator = _container.GetItemQueryIterator<Models.FinancementConformite.Opco.Opco>(dataQuery);

        while (dataIterator.HasMoreResults)
        {
            var resultPage = await dataIterator.ReadNextAsync(cancellationToken);
            items.AddRange(resultPage);
        }

        return new PagedResult<Models.FinancementConformite.Opco.Opco>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IReadOnlyList<Models.FinancementConformite.Opco.Opco>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        const string queryText = "SELECT * FROM c WHERE c.IsActive = true";
        var queryDefinition = new QueryDefinition(queryText);

        var items = new List<Models.FinancementConformite.Opco.Opco>();
        using var iterator = _container.GetItemQueryIterator<Models.FinancementConformite.Opco.Opco>(queryDefinition);

        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync(cancellationToken);
            items.AddRange(page);
        }

        return items;
    }

    public async Task<Models.FinancementConformite.Opco.Opco> CreateAsync(Models.FinancementConformite.Opco.Opco opco, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(opco);

        var response = await _container.CreateItemAsync(
            opco, new PartitionKey(opco.Id), cancellationToken: cancellationToken);
        return response.Resource;
    }

    public async Task<Models.FinancementConformite.Opco.Opco> UpdateAsync(Models.FinancementConformite.Opco.Opco opco, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(opco);

        var response = await _container.ReplaceItemAsync(
            opco, opco.Id, new PartitionKey(opco.Id), cancellationToken: cancellationToken);
        return response.Resource;
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _container.DeleteItemAsync<Models.FinancementConformite.Opco.Opco>(
            id, new PartitionKey(id), cancellationToken: cancellationToken);
    }
}
