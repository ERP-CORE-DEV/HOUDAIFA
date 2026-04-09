using Training.SkillDevelopment.Models.CertificationEcosysteme.Certification;

namespace Training.SkillDevelopment.Services.CertificationEcosysteme.Certification;

public interface ICertificationService
{
    Task<CertificationRncp?> GetByIdAsync(string id);
    Task<IReadOnlyList<CertificationRncp>> GetAllAsync();
    Task<CertificationRncp?> GetByRncpCodeAsync(string rncpCode);
    Task<IReadOnlyList<CertificationRncp>> GetExpiringCertificationsAsync(int daysAhead = 90);
    Task<bool> ValidateRncpCodeAsync(string rncpCode);
    Task<CertificationRncp> CreateAsync(CertificationRncp certification);
    Task<CertificationRncp> UpdateAsync(CertificationRncp certification);
    Task DeleteAsync(string id);
}
