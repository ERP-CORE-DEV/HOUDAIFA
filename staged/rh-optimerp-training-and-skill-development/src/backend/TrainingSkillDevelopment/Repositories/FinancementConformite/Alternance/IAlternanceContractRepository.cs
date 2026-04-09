using Training.SkillDevelopment.Models.FinancementConformite.Alternance;

namespace Training.SkillDevelopment.Repositories.FinancementConformite.Alternance;

public interface IAlternanceContractRepository
{
    Task<AlternanceContract?> GetByIdAsync(string id);
    Task<IReadOnlyList<AlternanceContract>> GetAllAsync();
    Task<IReadOnlyList<AlternanceContract>> GetByEmployeeIdAsync(string employeeId);
    Task<IReadOnlyList<AlternanceContract>> GetActiveAsync();
    Task<AlternanceContract> AddAsync(AlternanceContract contract);
    Task<AlternanceContract> UpdateAsync(AlternanceContract contract);
    Task DeleteAsync(string id);
}
