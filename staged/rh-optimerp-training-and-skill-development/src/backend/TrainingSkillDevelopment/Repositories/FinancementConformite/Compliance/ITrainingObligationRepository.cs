using Training.SkillDevelopment.Models.FinancementConformite.Compliance;
using Training.SkillDevelopment.Models.Common;

namespace Training.SkillDevelopment.Repositories.FinancementConformite.Compliance;

public interface ITrainingObligationRepository
{
    Task<TrainingObligation?> GetByIdAsync(string id);
    Task<IReadOnlyList<TrainingObligation>> GetAllAsync();
    Task<IReadOnlyList<TrainingObligation>> GetActiveAsync();
    Task<IReadOnlyList<TrainingObligation>> GetByRegulatoryReferenceAsync(string regulatoryReference);
    Task<PagedResult<TrainingObligation>> GetPagedAsync(int page, int pageSize);
    Task<TrainingObligation> AddAsync(TrainingObligation obligation);
    Task<TrainingObligation> UpdateAsync(TrainingObligation obligation);
    Task DeleteAsync(string id);
}
