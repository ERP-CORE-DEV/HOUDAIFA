using Training.SkillDevelopment.Models.CertificationEcosysteme.Provider;

namespace Training.SkillDevelopment.Repositories.CertificationEcosysteme.Provider;

public interface ITrainingProviderRepository
{
    Task<TrainingProvider?> GetByIdAsync(string id);
    Task<IReadOnlyList<TrainingProvider>> GetAllAsync();
    Task<IReadOnlyList<TrainingProvider>> GetByQualiopiStatusAsync(bool hasQualiopi);
    Task<IReadOnlyList<TrainingProvider>> SearchByNameAsync(string searchTerm);
    Task<TrainingProvider> AddAsync(TrainingProvider provider);
    Task<TrainingProvider> UpdateAsync(TrainingProvider provider);
    Task DeleteAsync(string id);
}
