using Training.SkillDevelopment.Models.Common;
using TrainingActionEntity = Training.SkillDevelopment.Models.FormationExecution.TrainingAction.TrainingAction;

namespace Training.SkillDevelopment.Services.FormationExecution.TrainingAction;

public interface ITrainingActionService
{
    Task<TrainingActionEntity?> GetByIdAsync(string id);
    Task<PagedResult<TrainingActionEntity>> GetPagedAsync(int page, int pageSize);
    Task<IReadOnlyList<TrainingActionEntity>> GetByPlanIdAsync(string planId);
    Task<TrainingActionEntity> CreateAsync(TrainingActionEntity action);
    Task<TrainingActionEntity> UpdateAsync(TrainingActionEntity action);
    Task DeleteAsync(string id);
}
