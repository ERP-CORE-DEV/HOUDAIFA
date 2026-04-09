using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FinancementConformite.Compliance;

namespace Training.SkillDevelopment.Services.FinancementConformite.Compliance;

public interface ITrainingObligationService
{
    Task<TrainingObligation?> GetByIdAsync(string id);
    Task<IReadOnlyList<TrainingObligation>> GetAllAsync();
    Task<IReadOnlyList<TrainingObligation>> GetExpiringAsync(int daysAhead = 90);
    Task<IReadOnlyList<ComplianceAlert>> GetRiskAssessmentAsync();
    Task<TrainingObligation> CreateAsync(TrainingObligation obligation);
    Task<TrainingObligation> UpdateAsync(TrainingObligation obligation);
    Task DeleteAsync(string id);
}
