using System.ComponentModel.DataAnnotations;
using Training.SkillDevelopment.Models.Common;

namespace Training.SkillDevelopment.DTOs.FormationExecution.TrainingAction;

public sealed class TrainingActionDto
{
    public string Id { get; set; } = string.Empty;

    public string? PlanId { get; set; }

    [Required(ErrorMessage = "Le titre est requis.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Le titre doit contenir entre 1 et 200 caracteres.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Type { get; set; } = TrainingActionType.Developpement.ToString();

    public string? Category { get; set; }

    public string Modality { get; set; } = TrainingModality.Presentiel.ToString();

    [Range(1, int.MaxValue, ErrorMessage = "La duree doit etre superieure a 0.")]
    public int DurationHours { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Le cout doit etre positif ou nul.")]
    public decimal Cost { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Le nombre maximum de participants doit etre superieur a 0.")]
    public int MaxParticipants { get; set; }

    public string[]? Prerequisites { get; set; }

    public bool IsObligatory { get; set; }

    public string[]? CompetencyIds { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public bool IsActive { get; set; } = true;

    public static TrainingActionDto FromDomain(Models.FormationExecution.TrainingAction.TrainingAction action)
    {
        return new TrainingActionDto
        {
            Id = action.Id,
            PlanId = action.PlanId,
            Title = action.Title,
            Description = action.Description,
            Type = action.Type.ToString(),
            Category = action.Category,
            Modality = action.Modality.ToString(),
            DurationHours = action.DurationHours,
            Cost = action.Cost,
            MaxParticipants = action.MaxParticipants,
            Prerequisites = action.Prerequisites,
            IsObligatory = action.IsObligatory,
            CompetencyIds = action.CompetencyIds,
            CreatedAt = action.CreatedAt,
            CreatedBy = action.CreatedBy,
            UpdatedAt = action.UpdatedAt,
            UpdatedBy = action.UpdatedBy,
            IsActive = action.IsActive
        };
    }

    public Models.FormationExecution.TrainingAction.TrainingAction ToDomain()
    {
        if (!Enum.TryParse<TrainingActionType>(Type, out var type))
            throw new ArgumentException($"Type d'action invalide : '{Type}'.");

        if (!Enum.TryParse<TrainingModality>(Modality, out var modality))
            throw new ArgumentException($"Modalite invalide : '{Modality}'.");

        return new Models.FormationExecution.TrainingAction.TrainingAction
        {
            Id = Id,
            PlanId = PlanId,
            Title = Title,
            Description = Description,
            Type = type,
            Category = Category,
            Modality = modality,
            DurationHours = DurationHours,
            Cost = Cost,
            MaxParticipants = MaxParticipants,
            Prerequisites = Prerequisites,
            IsObligatory = IsObligatory,
            CompetencyIds = CompetencyIds,
            CreatedAt = CreatedAt,
            CreatedBy = CreatedBy,
            UpdatedAt = UpdatedAt,
            UpdatedBy = UpdatedBy,
            IsActive = IsActive
        };
    }
}
