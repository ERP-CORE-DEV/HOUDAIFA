using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FormationExecution.TrainingPlan;

namespace Training.SkillDevelopment.Services.FormationExecution.TrainingPlan;

public interface ITrainingPlanService
{
    Task<Models.FormationExecution.TrainingPlan.TrainingPlan?> GetByIdAsync(string id);
    Task<IReadOnlyList<Models.FormationExecution.TrainingPlan.TrainingPlan>> GetAllAsync();
    Task<IReadOnlyList<Models.FormationExecution.TrainingPlan.TrainingPlan>> GetByYearAsync(int year);
    Task<Models.FormationExecution.TrainingPlan.TrainingPlan> CreateAsync(Models.FormationExecution.TrainingPlan.TrainingPlan plan);
    Task<Models.FormationExecution.TrainingPlan.TrainingPlan> UpdateAsync(Models.FormationExecution.TrainingPlan.TrainingPlan plan);
    Task<bool> DeleteAsync(string id);
    Task<Models.FormationExecution.TrainingPlan.TrainingPlan> ApproveAsync(string id, string approvedBy);
    Task<TrainingBudget> GetBudgetSummaryAsync(string planId);
    Task<PagedResult<Models.FormationExecution.TrainingPlan.TrainingPlan>> GetPagedAsync(int page, int pageSize);
}
