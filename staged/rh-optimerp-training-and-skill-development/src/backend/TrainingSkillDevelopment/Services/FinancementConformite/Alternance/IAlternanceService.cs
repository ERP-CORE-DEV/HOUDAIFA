using Training.SkillDevelopment.Models.FinancementConformite.Alternance;
using Training.SkillDevelopment.Models.Common;

namespace Training.SkillDevelopment.Services.FinancementConformite.Alternance;

public interface IAlternanceService
{
    Task<AlternanceContract?> GetByIdAsync(string id);
    Task<IReadOnlyList<AlternanceContract>> GetByEmployeeIdAsync(string employeeId);
    Task<AlternanceContract> CreateAsync(AlternanceContract contract);
    Task<AlternanceContract> UpdateAsync(AlternanceContract contract);
    Task<decimal> CalculateRemunerationAsync(string contractId);
    Task<bool> CheckEligibilityAsync(string employeeId, DateTime birthDate);
    Task DeleteAsync(string id);
}
