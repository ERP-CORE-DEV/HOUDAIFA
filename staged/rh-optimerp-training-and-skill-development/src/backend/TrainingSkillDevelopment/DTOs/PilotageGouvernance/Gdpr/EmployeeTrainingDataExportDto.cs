using Training.SkillDevelopment.DTOs.Competency;
using Training.SkillDevelopment.DTOs.FinancementConformite.Cpf;
using Training.SkillDevelopment.DTOs.EntretienProfessionnel;

namespace Training.SkillDevelopment.DTOs.PilotageGouvernance.Gdpr;

public sealed class EmployeeTrainingDataExportDto
{
    public string EmployeeId { get; init; } = string.Empty;

    public DateTime ExportDate { get; init; }

    public IReadOnlyList<string> Enrollments { get; init; } = Array.Empty<string>();

    public IReadOnlyList<CpfTransactionDto> CpfTransactions { get; init; } = Array.Empty<CpfTransactionDto>();

    public IReadOnlyList<EmployeeCompetencyAssessmentDto> CompetencyAssessments { get; init; } = Array.Empty<EmployeeCompetencyAssessmentDto>();

    public IReadOnlyList<ProfessionalInterviewDto> Interviews { get; init; } = Array.Empty<ProfessionalInterviewDto>();

    public IReadOnlyList<string> Certifications { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> VaeProjects { get; init; } = Array.Empty<string>();
}
