using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FinancementConformite.Opco;

namespace Training.SkillDevelopment.Services.FinancementConformite.Opco;

public interface IFundingApplicationService
{
    Task<FundingApplication?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<PagedResult<FundingApplication>> GetByOpcoIdAsync(string opcoId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<FundingApplication>> GetByStatusAsync(FundingStatus status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<FundingApplication> SubmitAsync(FundingApplication application, CancellationToken cancellationToken = default);
    Task<FundingApplication> ApproveAsync(string id, decimal grantedAmount, CancellationToken cancellationToken = default);
    Task<FundingApplication> RejectAsync(string id, string rejectionReason, CancellationToken cancellationToken = default);
    Task<FundingApplication> UpdateAsync(FundingApplication application, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
