using Newtonsoft.Json;
using Training.SkillDevelopment.Models.Auditing;
using Training.SkillDevelopment.Models.Common;

namespace Training.SkillDevelopment.Models.FormationExecution.TrainingAction;

public sealed class TrainingAction : IAuditable, ISoftDeletable
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string? PlanId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TrainingActionType Type { get; set; }

    public string? Category { get; set; }

    public TrainingModality Modality { get; set; }

    public int DurationHours { get; set; }

    public decimal Cost { get; set; }

    public int MaxParticipants { get; set; }

    public string[]? Prerequisites { get; set; }

    public bool IsObligatory { get; set; }

    public string[]? CompetencyIds { get; set; }

    // IAuditable
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // ISoftDeletable
    public bool IsActive { get; set; } = true;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
