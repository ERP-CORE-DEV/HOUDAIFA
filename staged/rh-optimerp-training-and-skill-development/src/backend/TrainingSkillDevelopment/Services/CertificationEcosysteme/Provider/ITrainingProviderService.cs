using Training.SkillDevelopment.Models.CertificationEcosysteme.Provider;

namespace Training.SkillDevelopment.Services.CertificationEcosysteme.Provider;

public interface ITrainingProviderService
{
    Task<TrainingProvider?> GetByIdAsync(string id);
    Task<IReadOnlyList<TrainingProvider>> GetAllAsync();
    Task<IReadOnlyList<TrainingProvider>> SearchByNameAsync(string searchTerm);
    Task<bool> ValidateQualiopiAsync(string id);
    Task<decimal> GetProviderRatingAsync(string id);
    Task<TrainingProvider> CreateAsync(TrainingProvider provider);
    Task<TrainingProvider> UpdateAsync(TrainingProvider provider);
    Task DeleteAsync(string id);
}
