using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FormationExecution.ELearning;

namespace Training.SkillDevelopment.Repositories.FormationExecution.ELearning;

public interface IElearningCourseRepository
{
    Task<ElearningCourse?> GetByIdAsync(string id);
    Task<IReadOnlyList<ElearningCourse>> GetAllAsync();
    Task<IReadOnlyList<ElearningCourse>> GetByTagAsync(string tag);
    Task<IReadOnlyList<ElearningCourse>> GetByCompetencyIdAsync(string competencyId);
    Task<ElearningCourse> AddAsync(ElearningCourse course);
    Task<ElearningCourse> UpdateAsync(ElearningCourse course);
    Task<bool> DeleteAsync(string id);
    Task<PagedResult<ElearningCourse>> GetPagedAsync(int page, int pageSize);
}
