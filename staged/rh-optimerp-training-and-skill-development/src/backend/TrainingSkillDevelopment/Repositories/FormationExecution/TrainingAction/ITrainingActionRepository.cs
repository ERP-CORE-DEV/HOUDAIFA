using Training.SkillDevelopment.Models.Common;
using TrainingActionEntity = Training.SkillDevelopment.Models.FormationExecution.TrainingAction.TrainingAction;

namespace Training.SkillDevelopment.Repositories.FormationExecution.TrainingAction;

public interface ITrainingActionRepository
{
    Task<TrainingActionEntity?> GetByIdAsync(string id);
    Task<IReadOnlyList<TrainingActionEntity>> GetAllAsync();
    Task<IReadOnlyList<TrainingActionEntity>> GetByPlanIdAsync(string planId);
    Task<PagedResult<TrainingActionEntity>> GetPagedAsync(int page, int pageSize);
    Task<TrainingActionEntity> AddAsync(TrainingActionEntity action);
    Task<TrainingActionEntity> UpdateAsync(TrainingActionEntity action);
    Task DeleteAsync(string id);
}
