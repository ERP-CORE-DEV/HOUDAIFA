using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Training.SkillDevelopment.Configuration;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FinancementConformite.Opco;

namespace Training.SkillDevelopment.Repositories.FinancementConformite.Opco;

public sealed class CosmosFundingApplicationRepository : IFundingApplicationRepository
{
    private readonly Container _container;

    public CosmosFundingApplicationRepository(CosmosClient cosmosClient, IOptions<CosmosDbSettings> settings)
    {
        ArgumentNullException.ThrowIfNull(cosmosClient);
        ArgumentNullException.ThrowIfNull(settings);

        var dbSettings = settings.Value;
        _container = cosmosClient.GetContainer(
            dbSettings.DatabaseName,
            dbSettings.Containers["FundingApplications"]);
    }

    public async Task<FundingApplication?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<FundingApplication>(
                id, new PartitionKey(id), cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<PagedResult<FundingApplication>> GetByOpcoIdAsync(string opcoId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var countQuery = new QueryDefinition(
            "SELECT VALUE COUNT(1) FROM c WHERE c.OpcoId = @opcoId")
            .WithParameter("@opcoId", opcoId);

        var countIterator = _container.GetItemQueryIterator<int>(countQuery);
        var countPage = await countIterator.ReadNextAsync(cancellationToken);
        var totalCount = countPage.FirstOrDefault();

        var offset = (page - 1) * pageSize;
        var dataQuery = new QueryDefinition(
            "SELECT * FROM c WHERE c.OpcoId = @opcoId ORDER BY c.SubmissionDate DESC OFFSET @offset LIMIT @limit")
            .WithParameter("@opcoId", opcoId)
            .WithParameter("@offset", offset)
            .WithParameter("@limit", pageSize);

        var items = new List<FundingApplication>();
        using var dataIterator = _container.GetItemQueryIterator<FundingApplication>(dataQuery);

        while (dataIterator.HasMoreResults)
        {
            var resultPage = await dataIterator.ReadNextAsync(cancellationToken);
            items.AddRange(resultPage);
        }

        return new PagedResult<FundingApplication>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<FundingApplication>> GetByStatusAsync(FundingStatus status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var statusString = status.ToString();

        var countQuery = new QueryDefinition(
            "SELECT VALUE COUNT(1) FROM c WHERE c.Status = @status")
            .WithParameter("@status", statusString);

        var countIterator = _container.GetItemQueryIterator<int>(countQuery);
        var countPage = await countIterator.ReadNextAsync(cancellationToken);
        var totalCount = countPage.FirstOrDefault();

        var offset = (page - 1) * pageSize;
        var dataQuery = new QueryDefinition(
            "SELECT * FROM c WHERE c.Status = @status ORDER BY c.SubmissionDate DESC OFFSET @offset LIMIT @limit")
            .WithParameter("@status", statusString)
            .WithParameter("@offset", offset)
            .WithParameter("@limit", pageSize);

        var items = new List<FundingApplication>();
        using var dataIterator = _container.GetItemQueryIterator<FundingApplication>(dataQuery);

        while (dataIterator.HasMoreResults)
        {
            var resultPage = await dataIterator.ReadNextAsync(cancellationToken);
            items.AddRange(resultPage);
        }

        return new PagedResult<FundingApplication>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IReadOnlyList<FundingApplication>> GetByTrainingActionIdAsync(string trainingActionId, CancellationToken cancellationToken = default)
    {
        var queryDefinition = new QueryDefinition(
            "SELECT * FROM c WHERE c.TrainingActionId = @trainingActionId")
            .WithParameter("@trainingActionId", trainingActionId);

        var items = new List<FundingApplication>();
        using var iterator = _container.GetItemQueryIterator<FundingApplication>(queryDefinition);

        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync(cancellationToken);
            items.AddRange(page);
        }

        return items;
    }

    public async Task<FundingApplication> CreateAsync(FundingApplication application, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(application);

        var response = await _container.CreateItemAsync(
            application, new PartitionKey(application.Id), cancellationToken: cancellationToken);
        return response.Resource;
    }

    public async Task<FundingApplication> UpdateAsync(FundingApplication application, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(application);

        var response = await _container.ReplaceItemAsync(
            application, application.Id, new PartitionKey(application.Id), cancellationToken: cancellationToken);
        return response.Resource;
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _container.DeleteItemAsync<FundingApplication>(
            id, new PartitionKey(id), cancellationToken: cancellationToken);
    }
}
