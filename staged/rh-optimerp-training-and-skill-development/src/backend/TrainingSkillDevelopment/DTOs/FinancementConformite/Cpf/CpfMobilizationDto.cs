using System.ComponentModel.DataAnnotations;
using Training.SkillDevelopment.Models.FinancementConformite.Cpf;

namespace Training.SkillDevelopment.DTOs.FinancementConformite.Cpf;

public sealed class CpfMobilizationDto
{
    public string Id { get; init; } = string.Empty;

    [Required(ErrorMessage = "L'identifiant du compte CPF est obligatoire.")]
    public string AccountId { get; init; } = string.Empty;

    [Required(ErrorMessage = "L'identifiant de l'action de formation est obligatoire.")]
    public string TrainingActionId { get; init; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Le montant de la mobilisation doit être supérieur à zéro.")]
    public decimal Amount { get; init; }

    public bool DuringWorkingHours { get; init; }

    public bool? EmployerApproval { get; init; }

    [Range(0, double.MaxValue, ErrorMessage = "Le reste à charge ne peut pas être négatif.")]
    public decimal ResteACharge { get; init; }

    [Required(ErrorMessage = "Le statut de la mobilisation est obligatoire.")]
    public string Status { get; init; } = string.Empty;

    public DateTime RequestDate { get; init; }

    public DateTime? ApprovalDate { get; init; }

    public static CpfMobilizationDto FromDomain(CpfMobilization mobilization)
    {
        ArgumentNullException.ThrowIfNull(mobilization);

        return new CpfMobilizationDto
        {
            Id = mobilization.Id,
            AccountId = mobilization.AccountId,
            TrainingActionId = mobilization.TrainingActionId,
            Amount = mobilization.Amount,
            DuringWorkingHours = mobilization.DuringWorkingHours,
            EmployerApproval = mobilization.EmployerApproval,
            ResteACharge = mobilization.ResteACharge,
            Status = mobilization.Status,
            RequestDate = mobilization.RequestDate,
            ApprovalDate = mobilization.ApprovalDate
        };
    }

    public CpfMobilization ToDomain()
    {
        return new CpfMobilization
        {
            Id = Id,
            AccountId = AccountId,
            TrainingActionId = TrainingActionId,
            Amount = Amount,
            DuringWorkingHours = DuringWorkingHours,
            EmployerApproval = EmployerApproval,
            ResteACharge = ResteACharge,
            Status = Status,
            RequestDate = RequestDate,
            ApprovalDate = ApprovalDate
        };
    }
}
