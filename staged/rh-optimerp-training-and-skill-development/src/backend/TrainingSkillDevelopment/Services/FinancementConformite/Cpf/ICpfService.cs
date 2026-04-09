using Training.SkillDevelopment.Models.FinancementConformite.Cpf;

namespace Training.SkillDevelopment.Services.FinancementConformite.Cpf;

public interface ICpfService
{
    Task<CpfAccount?> GetAccountByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<CpfAccount?> GetAccountByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default);
    Task<CpfAccount> CreateAccountAsync(CpfAccount account, CancellationToken cancellationToken = default);
    Task<CpfAccount> ApplyAnnualCreditAsync(string accountId, CancellationToken cancellationToken = default);
    Task<CpfMobilization> MobilizeCpfAsync(CpfMobilization mobilization, CancellationToken cancellationToken = default);
    Task<CpfAccount> ApplyAbondementCorrectifAsync(string accountId, CancellationToken cancellationToken = default);
    decimal CalculateAnnualCredit(bool isLowQualified);
    decimal CalculateCeiling(bool isLowQualified);
    decimal ApplyResteACharge(decimal mobilizationAmount);
}
