using Training.SkillDevelopment.Models.FormationExecution.Evaluation;

namespace Training.SkillDevelopment.Repositories.FormationExecution.Evaluation;

public interface ITrainingEvaluationRepository
{
    Task<TrainingEvaluation?> GetByIdAsync(string id);
    Task<IReadOnlyList<TrainingEvaluation>> GetBySessionIdAsync(string sessionId);
    Task<IReadOnlyList<TrainingEvaluation>> GetByEmployeeIdAsync(string employeeId);
    Task<decimal> GetAverageScoreAsync(string sessionId);
    Task<TrainingEvaluation> AddAsync(TrainingEvaluation evaluation);
    Task<TrainingEvaluation> UpdateAsync(TrainingEvaluation evaluation);
    Task DeleteAsync(string id);
}
