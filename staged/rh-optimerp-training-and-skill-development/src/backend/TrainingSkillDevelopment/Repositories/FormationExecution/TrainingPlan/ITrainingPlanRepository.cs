using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FormationExecution.TrainingPlan;

namespace Training.SkillDevelopment.Repositories.FormationExecution.TrainingPlan;

public interface ITrainingPlanRepository
{
    Task<Models.FormationExecution.TrainingPlan.TrainingPlan?> GetByIdAsync(string id);
    Task<IReadOnlyList<Models.FormationExecution.TrainingPlan.TrainingPlan>> GetAllAsync();
    Task<IReadOnlyList<Models.FormationExecution.TrainingPlan.TrainingPlan>> GetByYearAsync(int year);
    Task<IReadOnlyList<Models.FormationExecution.TrainingPlan.TrainingPlan>> GetByCompanyIdAsync(string companyId);
    Task<IReadOnlyList<Models.FormationExecution.TrainingPlan.TrainingPlan>> GetByStatusAsync(TrainingPlanStatus status);
    Task<Models.FormationExecution.TrainingPlan.TrainingPlan> AddAsync(Models.FormationExecution.TrainingPlan.TrainingPlan plan);
    Task<Models.FormationExecution.TrainingPlan.TrainingPlan> UpdateAsync(Models.FormationExecution.TrainingPlan.TrainingPlan plan);
    Task<bool> DeleteAsync(string id);
    Task<PagedResult<Models.FormationExecution.TrainingPlan.TrainingPlan>> GetPagedAsync(int page, int pageSize);
}
