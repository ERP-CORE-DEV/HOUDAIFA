using Microsoft.Azure.Cosmos;
using Training.SkillDevelopment.Models.FinancementConformite.Cpf;

namespace Training.SkillDevelopment.Repositories.FinancementConformite.Cpf;

public sealed class CosmosCpfAccountRepository : ICpfAccountRepository
{
    private readonly Container _container;

    public CosmosCpfAccountRepository(Container container)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
    }

    public async Task<CpfAccount?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        try
        {
            var response = await _container.ReadItemAsync<CpfAccount>(
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

    public async Task<CpfAccount?> GetByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(employeeId);

        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.employeeId = @employeeId AND c.isActive = true")
            .WithParameter("@employeeId", employeeId);

        using var iterator = _container.GetItemQueryIterator<CpfAccount>(query);

        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync(cancellationToken);
            var item = page.FirstOrDefault();
            if (item is not null)
            {
                return item;
            }
        }

        return null;
    }

    public async Task<CpfAccount> CreateAsync(CpfAccount account, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(account);

        var response = await _container.CreateItemAsync(
            account,
            new PartitionKey(account.Id),
            cancellationToken: cancellationToken);

        return response.Resource;
    }

    public async Task<CpfAccount> UpdateAsync(CpfAccount account, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(account);

        var response = await _container.ReplaceItemAsync(
            account,
            account.Id,
            new PartitionKey(account.Id),
            cancellationToken: cancellationToken);

        return response.Resource;
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        await _container.DeleteItemAsync<CpfAccount>(
            id,
            new PartitionKey(id),
            cancellationToken: cancellationToken);
    }
}
