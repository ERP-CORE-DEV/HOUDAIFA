using Training.SkillDevelopment.Models.FormationExecution.TrainingAction;

namespace Training.SkillDevelopment.Repositories.FormationExecution.TrainingAction;

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetByIdAsync(string id);
    Task<IReadOnlyList<Enrollment>> GetBySessionIdAsync(string sessionId);
    Task<IReadOnlyList<Enrollment>> GetByEmployeeIdAsync(string employeeId);
    Task<Enrollment> AddAsync(Enrollment enrollment);
    Task<Enrollment> UpdateAsync(Enrollment enrollment);
    Task DeleteAsync(string id);
}
