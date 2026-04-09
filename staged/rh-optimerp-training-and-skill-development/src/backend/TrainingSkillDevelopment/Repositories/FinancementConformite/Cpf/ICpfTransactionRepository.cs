using Training.SkillDevelopment.Models.FinancementConformite.Cpf;

namespace Training.SkillDevelopment.Repositories.FinancementConformite.Cpf;

public interface ICpfTransactionRepository
{
    Task<CpfTransaction?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CpfTransaction>> GetByAccountIdAsync(string accountId, CancellationToken cancellationToken = default);
    Task<CpfTransaction> CreateAsync(CpfTransaction transaction, CancellationToken cancellationToken = default);
}
