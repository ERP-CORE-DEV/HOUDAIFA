using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FinancementConformite.Opco;

namespace Training.SkillDevelopment.Repositories.FinancementConformite.Opco;

public interface IFundingApplicationRepository
{
    Task<FundingApplication?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<PagedResult<FundingApplication>> GetByOpcoIdAsync(string opcoId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<FundingApplication>> GetByStatusAsync(FundingStatus status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FundingApplication>> GetByTrainingActionIdAsync(string trainingActionId, CancellationToken cancellationToken = default);
    Task<FundingApplication> CreateAsync(FundingApplication application, CancellationToken cancellationToken = default);
    Task<FundingApplication> UpdateAsync(FundingApplication application, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
