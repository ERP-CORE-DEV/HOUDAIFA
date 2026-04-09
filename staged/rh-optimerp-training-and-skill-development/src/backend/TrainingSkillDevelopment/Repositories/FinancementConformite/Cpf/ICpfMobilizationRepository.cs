using Training.SkillDevelopment.Models.FinancementConformite.Cpf;

namespace Training.SkillDevelopment.Repositories.FinancementConformite.Cpf;

public interface ICpfMobilizationRepository
{
    Task<CpfMobilization?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CpfMobilization>> GetByAccountIdAsync(string accountId, CancellationToken cancellationToken = default);
    Task<CpfMobilization> CreateAsync(CpfMobilization mobilization, CancellationToken cancellationToken = default);
    Task<CpfMobilization> UpdateAsync(CpfMobilization mobilization, CancellationToken cancellationToken = default);
}
