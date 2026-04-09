using System.ComponentModel.DataAnnotations;
using Training.SkillDevelopment.Models.FinancementConformite.Alternance;
using Training.SkillDevelopment.Models.Common;

namespace Training.SkillDevelopment.DTOs.FinancementConformite.Alternance;

public sealed class AlternanceContractDto
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'identifiant de l'employe est obligatoire.")]
    public string EmployeeId { get; set; } = string.Empty;

    public string Type { get; set; } = AlternanceContractType.Apprentissage.ToString();

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string? CfaId { get; set; }

    public string? TutorId { get; set; }

    public string? CertificationTargetId { get; set; }

    [Range(0, 100, ErrorMessage = "Le pourcentage de remuneration doit etre compris entre 0 et 100.")]
    public decimal RemunerationPercentage { get; set; }

    public string Status { get; set; } = string.Empty;

    public bool IsAnonymized { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public static AlternanceContractDto FromDomain(AlternanceContract contract)
    {
        ArgumentNullException.ThrowIfNull(contract);

        return new AlternanceContractDto
        {
            Id = contract.Id,
            EmployeeId = contract.EmployeeId,
            Type = contract.Type.ToString(),
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            CfaId = contract.CfaId,
            TutorId = contract.TutorId,
            CertificationTargetId = contract.CertificationTargetId,
            RemunerationPercentage = contract.RemunerationPercentage,
            Status = contract.Status,
            IsAnonymized = contract.IsAnonymized,
            CreatedAt = contract.CreatedAt,
            UpdatedAt = contract.UpdatedAt
        };
    }

    public AlternanceContract ToDomain()
    {
        if (!Enum.TryParse<AlternanceContractType>(Type, out var type))
            throw new ArgumentException($"Type de contrat d'alternance invalide : '{Type}'.");

        return new AlternanceContract
        {
            Id = Id,
            EmployeeId = EmployeeId,
            Type = type,
            StartDate = StartDate,
            EndDate = EndDate,
            CfaId = CfaId,
            TutorId = TutorId,
            CertificationTargetId = CertificationTargetId,
            RemunerationPercentage = RemunerationPercentage,
            Status = Status,
            IsAnonymized = IsAnonymized,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };
    }
}
