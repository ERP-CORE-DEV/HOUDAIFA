using Training.SkillDevelopment.Models.FormationExecution.TrainingAction;

namespace Training.SkillDevelopment.Repositories.FormationExecution.TrainingAction;

public interface ITrainingSessionRepository
{
    Task<TrainingSession?> GetByIdAsync(string id);
    Task<IReadOnlyList<TrainingSession>> GetByActionIdAsync(string actionId);
    Task<IReadOnlyList<TrainingSession>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<TrainingSession> AddAsync(TrainingSession session);
    Task<TrainingSession> UpdateAsync(TrainingSession session);
    Task DeleteAsync(string id);
}
