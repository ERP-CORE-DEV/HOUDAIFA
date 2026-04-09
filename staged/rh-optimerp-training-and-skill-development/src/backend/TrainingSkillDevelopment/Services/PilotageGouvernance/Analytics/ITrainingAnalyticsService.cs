using Training.SkillDevelopment.DTOs.PilotageGouvernance.Analytics;

namespace Training.SkillDevelopment.Services.PilotageGouvernance.Analytics;

public interface ITrainingAnalyticsService
{
    Task<TrainingDashboardKpiDto> GetDashboardKpisAsync(int year);
    Task<BilanSocialTrainingDto> GetBilanSocialTrainingAsync(int year);
    Task<GenderEqualityTrainingReportDto> GetGenderEqualityReportAsync(int year);
    Task<decimal> CalculateTrainingRoiAsync(string trainingActionId);
    Task<IReadOnlyList<TrainingTrendDto>> GetTrainingTrendsAsync(int startYear, int endYear);
}
