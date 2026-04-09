using Training.SkillDevelopment.Models.CertificationEcosysteme.Certification;
using Training.SkillDevelopment.Models.Common;

namespace Training.SkillDevelopment.Services.CertificationEcosysteme.Certification;

public interface IVaeProjectService
{
    Task<VaeProject?> GetByIdAsync(string id);
    Task<IReadOnlyList<VaeProject>> GetByEmployeeIdAsync(string employeeId);
    Task<VaeProject> CreateAsync(VaeProject project);
    Task<VaeProject> UpdateAsync(VaeProject project);
    Task<VaeProject> AdvancePhaseAsync(string id);
    Task DeleteAsync(string id);
}
