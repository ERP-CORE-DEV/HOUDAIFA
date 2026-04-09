using Training.SkillDevelopment.Models.CertificationEcosysteme.Certification;

namespace Training.SkillDevelopment.Repositories.CertificationEcosysteme.Certification;

public interface ICertificationRepository
{
    Task<CertificationRncp?> GetByIdAsync(string id);
    Task<IReadOnlyList<CertificationRncp>> GetAllAsync();
    Task<CertificationRncp?> GetByRncpCodeAsync(string rncpCode);
    Task<CertificationRncp> AddAsync(CertificationRncp cert);
    Task<CertificationRncp> UpdateAsync(CertificationRncp cert);
    Task DeleteAsync(string id);
}
