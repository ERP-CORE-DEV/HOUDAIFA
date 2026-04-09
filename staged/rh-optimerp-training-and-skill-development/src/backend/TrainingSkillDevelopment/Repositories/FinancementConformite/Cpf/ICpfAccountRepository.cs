using Training.SkillDevelopment.Models.FinancementConformite.Cpf;

namespace Training.SkillDevelopment.Repositories.FinancementConformite.Cpf;

public interface ICpfAccountRepository
{
    Task<CpfAccount?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<CpfAccount?> GetByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default);
    Task<CpfAccount> CreateAsync(CpfAccount account, CancellationToken cancellationToken = default);
    Task<CpfAccount> UpdateAsync(CpfAccount account, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
