using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FinancementConformite.Opco;

namespace Training.SkillDevelopment.Services.FinancementConformite.Opco;

public interface IOpcoService
{
    Task<Models.FinancementConformite.Opco.Opco?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Models.FinancementConformite.Opco.Opco?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<PagedResult<Models.FinancementConformite.Opco.Opco>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Models.FinancementConformite.Opco.Opco>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Models.FinancementConformite.Opco.Opco> CreateAsync(Models.FinancementConformite.Opco.Opco opco, CancellationToken cancellationToken = default);
    Task<Models.FinancementConformite.Opco.Opco> UpdateAsync(Models.FinancementConformite.Opco.Opco opco, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<TrainingContribution> CalculateContributionAsync(string companyId, int year, decimal masseSalariale, int headcount, CancellationToken cancellationToken = default);
    Task<bool> IsEligibleForFundingAsync(string opcoId, string trainingActionId, CancellationToken cancellationToken = default);
}
