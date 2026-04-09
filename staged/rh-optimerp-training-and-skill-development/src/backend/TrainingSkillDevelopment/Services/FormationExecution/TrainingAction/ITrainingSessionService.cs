using Training.SkillDevelopment.Models.FormationExecution.TrainingAction;

namespace Training.SkillDevelopment.Services.FormationExecution.TrainingAction;

public interface ITrainingSessionService
{
    Task<TrainingSession?> GetByIdAsync(string id);
    Task<IReadOnlyList<TrainingSession>> GetByActionIdAsync(string actionId);
    Task<IReadOnlyList<TrainingSession>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<TrainingSession> CreateAsync(TrainingSession session);
    Task<TrainingSession> UpdateAsync(TrainingSession session);
    Task DeleteAsync(string id);
}
