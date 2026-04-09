using System.ComponentModel.DataAnnotations;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FormationExecution.TrainingPlan;

namespace Training.SkillDevelopment.DTOs.FormationExecution.TrainingPlan;

public sealed class TrainingPlanDto
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'identifiant de l'entreprise est requis.")]
    public string CompanyId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le titre est requis.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Le titre doit contenir entre 1 et 200 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [Range(2000, 2100, ErrorMessage = "L'annee est requise et doit etre valide.")]
    public int Year { get; set; }

    public string Status { get; set; } = TrainingPlanStatus.Draft.ToString();

    public int Version { get; set; } = 1;

    [Range(0, double.MaxValue, ErrorMessage = "Le budget alloue doit etre positif.")]
    public decimal BudgetAllocated { get; set; }

    public decimal BudgetConsumed { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "La masse salariale doit etre positive.")]
    public decimal MasseSalariale { get; set; }

    public decimal LegalObligationRate { get; set; } = 0.01m;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }

    public bool IsActive { get; set; } = true;

    public static TrainingPlanDto FromDomain(Models.FormationExecution.TrainingPlan.TrainingPlan plan)
    {
        return new TrainingPlanDto
        {
            Id = plan.Id,
            CompanyId = plan.CompanyId,
            Title = plan.Title,
            Year = plan.Year,
            Status = plan.Status.ToString(),
            Version = plan.Version,
            BudgetAllocated = plan.BudgetAllocated,
            BudgetConsumed = plan.BudgetConsumed,
            MasseSalariale = plan.MasseSalariale,
            LegalObligationRate = plan.LegalObligationRate,
            Description = plan.Description,
            CreatedAt = plan.CreatedAt,
            CreatedBy = plan.CreatedBy,
            UpdatedAt = plan.UpdatedAt,
            UpdatedBy = plan.UpdatedBy,
            ApprovedAt = plan.ApprovedAt,
            ApprovedBy = plan.ApprovedBy,
            IsActive = plan.IsActive
        };
    }

    public Models.FormationExecution.TrainingPlan.TrainingPlan ToDomain()
    {
        if (!Enum.TryParse<TrainingPlanStatus>(Status, out var status))
            throw new ArgumentException($"Statut de plan invalide : '{Status}'.");

        return new Models.FormationExecution.TrainingPlan.TrainingPlan
        {
            Id = Id,
            CompanyId = CompanyId,
            Title = Title,
            Year = Year,
            Status = status,
            Version = Version,
            BudgetAllocated = BudgetAllocated,
            BudgetConsumed = BudgetConsumed,
            MasseSalariale = MasseSalariale,
            LegalObligationRate = LegalObligationRate,
            Description = Description,
            CreatedAt = CreatedAt,
            CreatedBy = CreatedBy,
            UpdatedAt = UpdatedAt,
            UpdatedBy = UpdatedBy,
            ApprovedAt = ApprovedAt,
            ApprovedBy = ApprovedBy,
            IsActive = IsActive
        };
    }
}
