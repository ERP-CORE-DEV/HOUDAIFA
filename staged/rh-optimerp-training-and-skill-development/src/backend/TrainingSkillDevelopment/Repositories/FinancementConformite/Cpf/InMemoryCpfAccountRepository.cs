using System.Collections.Concurrent;
using Training.SkillDevelopment.Models.FinancementConformite.Cpf;

namespace Training.SkillDevelopment.Repositories.FinancementConformite.Cpf;

public sealed class InMemoryCpfAccountRepository : ICpfAccountRepository
{
    private readonly ConcurrentDictionary<string, CpfAccount> _store = new();

    public Task<CpfAccount?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        _store.TryGetValue(id, out var account);
        return Task.FromResult(account);
    }

    public Task<CpfAccount?> GetByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(employeeId);

        var account = _store.Values.FirstOrDefault(a => a.EmployeeId == employeeId && a.IsActive);
        return Task.FromResult(account);
    }

    public Task<CpfAccount> CreateAsync(CpfAccount account, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(account);

        _store[account.Id] = account;
        return Task.FromResult(account);
    }

    public Task<CpfAccount> UpdateAsync(CpfAccount account, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(account);

        _store[account.Id] = account;
        return Task.FromResult(account);
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        _store.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
