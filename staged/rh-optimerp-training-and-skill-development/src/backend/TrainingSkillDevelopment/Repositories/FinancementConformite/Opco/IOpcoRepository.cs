using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FinancementConformite.Opco;

namespace Training.SkillDevelopment.Repositories.FinancementConformite.Opco;

public interface IOpcoRepository
{
    Task<Models.FinancementConformite.Opco.Opco?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Models.FinancementConformite.Opco.Opco?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<PagedResult<Models.FinancementConformite.Opco.Opco>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Models.FinancementConformite.Opco.Opco>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Models.FinancementConformite.Opco.Opco> CreateAsync(Models.FinancementConformite.Opco.Opco opco, CancellationToken cancellationToken = default);
    Task<Models.FinancementConformite.Opco.Opco> UpdateAsync(Models.FinancementConformite.Opco.Opco opco, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
