using Training.SkillDevelopment.Models.FormationExecution.Evaluation;

namespace Training.SkillDevelopment.Services.FormationExecution.Evaluation;

public interface ITrainingEvaluationService
{
    Task<TrainingEvaluation?> GetByIdAsync(string id);
    Task<IReadOnlyList<TrainingEvaluation>> GetBySessionIdAsync(string sessionId);
    Task<IReadOnlyList<TrainingEvaluation>> GetByEmployeeIdAsync(string employeeId);
    Task<decimal> GetAverageScoreAsync(string sessionId);
    Task<decimal> CalculateRoiAsync(string sessionId);
    Task<TrainingEvaluation> CreateAsync(TrainingEvaluation evaluation);
    Task<TrainingEvaluation> UpdateAsync(TrainingEvaluation evaluation);
    Task DeleteAsync(string id);
}
