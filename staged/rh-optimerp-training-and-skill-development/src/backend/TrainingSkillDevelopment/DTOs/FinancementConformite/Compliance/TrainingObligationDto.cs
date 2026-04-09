using System.ComponentModel.DataAnnotations;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FinancementConformite.Compliance;

namespace Training.SkillDevelopment.DTOs.FinancementConformite.Compliance;

public sealed class TrainingObligationDto
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le titre de l'obligation est obligatoire.")]
    [StringLength(200, ErrorMessage = "Le titre ne peut pas depasser 200 caracteres.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? RegulatoryReference { get; set; }

    public string? TargetJobFamily { get; set; }

    [Range(1, 1200, ErrorMessage = "La frequence doit etre comprise entre 1 et 1200 mois.")]
    public int FrequencyMonths { get; set; }

    public RiskLevel RiskLevel { get; set; }

    public string Status { get; set; } = ObligationStatus.Current.ToString();

    public DateTime EffectiveDate { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public static TrainingObligationDto FromDomain(TrainingObligation obligation)
    {
        ArgumentNullException.ThrowIfNull(obligation);

        return new TrainingObligationDto
        {
            Id = obligation.Id,
            Title = obligation.Title,
            Description = obligation.Description,
            RegulatoryReference = obligation.RegulatoryReference,
            TargetJobFamily = obligation.TargetJobFamily,
            FrequencyMonths = obligation.FrequencyMonths,
            RiskLevel = obligation.RiskLevel,
            Status = obligation.Status.ToString(),
            EffectiveDate = obligation.EffectiveDate,
            ExpirationDate = obligation.ExpirationDate,
            IsActive = obligation.IsActive,
            CreatedAt = obligation.CreatedAt,
            UpdatedAt = obligation.UpdatedAt
        };
    }

    public TrainingObligation ToDomain()
    {
        if (!Enum.TryParse<ObligationStatus>(Status, out var status))
            throw new ArgumentException($"Statut d'obligation invalide : '{Status}'.");

        return new TrainingObligation
        {
            Id = Id,
            Title = Title,
            Description = Description,
            RegulatoryReference = RegulatoryReference,
            TargetJobFamily = TargetJobFamily,
            FrequencyMonths = FrequencyMonths,
            RiskLevel = RiskLevel,
            Status = status,
            EffectiveDate = EffectiveDate,
            ExpirationDate = ExpirationDate,
            IsActive = IsActive,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };
    }
}
