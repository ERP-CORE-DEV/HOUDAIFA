using System.Collections.Concurrent;
using Training.SkillDevelopment.Models.Common;

namespace Training.SkillDevelopment.Repositories.FinancementConformite.Opco;

public sealed class InMemoryOpcoRepository : IOpcoRepository
{
    private readonly ConcurrentDictionary<string, Models.FinancementConformite.Opco.Opco> _store = new();

    public Task<Models.FinancementConformite.Opco.Opco?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var opco);
        return Task.FromResult(opco);
    }

    public Task<Models.FinancementConformite.Opco.Opco?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var opco = _store.Values.FirstOrDefault(o => o.Code == code);
        return Task.FromResult(opco);
    }

    public Task<PagedResult<Models.FinancementConformite.Opco.Opco>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var all = _store.Values.ToList();
        var items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult(new PagedResult<Models.FinancementConformite.Opco.Opco>
        {
            Items = items,
            TotalCount = all.Count,
            Page = page,
            PageSize = pageSize
        });
    }

    public Task<IReadOnlyList<Models.FinancementConformite.Opco.Opco>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Models.FinancementConformite.Opco.Opco> result = _store.Values.Where(o => o.IsActive).ToList();
        return Task.FromResult(result);
    }

    public Task<Models.FinancementConformite.Opco.Opco> CreateAsync(Models.FinancementConformite.Opco.Opco opco, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(opco);

        opco.Id = opco.Id == string.Empty ? Guid.NewGuid().ToString() : opco.Id;
        _store[opco.Id] = opco;
        return Task.FromResult(opco);
    }

    public Task<Models.FinancementConformite.Opco.Opco> UpdateAsync(Models.FinancementConformite.Opco.Opco opco, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(opco);

        _store[opco.Id] = opco;
        return Task.FromResult(opco);
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        _store.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
