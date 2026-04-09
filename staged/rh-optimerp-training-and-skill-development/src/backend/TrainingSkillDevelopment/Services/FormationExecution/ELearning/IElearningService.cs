using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FormationExecution.ELearning;

namespace Training.SkillDevelopment.Services.FormationExecution.ELearning;

public interface IElearningService
{
    Task<ElearningCourse?> GetByIdAsync(string id);
    Task<PagedResult<ElearningCourse>> GetPagedAsync(int page, int pageSize);
    Task<IReadOnlyList<ElearningCourse>> GetByTagAsync(string tag);
    Task<ElearningCourse> CreateAsync(ElearningCourse course);
    Task<ElearningCourse> UpdateAsync(ElearningCourse course);
    Task<LearnerProgress> TrackProgressAsync(LearnerProgress progress);
    Task<decimal> GetCompletionRateAsync(string courseId);
    Task DeleteAsync(string id);
}
