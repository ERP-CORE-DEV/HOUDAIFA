using Training.SkillDevelopment.Models.CertificationEcosysteme.Certification;

namespace Training.SkillDevelopment.Repositories.CertificationEcosysteme.Certification;

public interface IVaeProjectRepository
{
    Task<VaeProject?> GetByIdAsync(string id);
    Task<IReadOnlyList<VaeProject>> GetByEmployeeIdAsync(string employeeId);
    Task<VaeProject> AddAsync(VaeProject project);
    Task<VaeProject> UpdateAsync(VaeProject project);
    Task DeleteAsync(string id);
}
