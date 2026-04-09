using Training.SkillDevelopment.DTOs.PilotageGouvernance.Gdpr;

namespace Training.SkillDevelopment.Services.PilotageGouvernance.Gdpr;

public interface IGdprService
{
    Task AnonymizeEmployeeDataAsync(string employeeId);
    Task<EmployeeTrainingDataExportDto> ExportEmployeeTrainingDataAsync(string employeeId);
    Task<DataRetentionReportDto> GetDataRetentionStatusAsync();
}
