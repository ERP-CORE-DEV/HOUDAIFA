using Newtonsoft.Json;
using Training.SkillDevelopment.Models.Common;

namespace Training.SkillDevelopment.Models.FormationExecution.TrainingPlan;

public sealed class TrainingPlanLine
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string PlanId { get; set; } = string.Empty;

    public string? TrainingActionId { get; set; }

    public string Title { get; set; } = string.Empty;

    public int Priority { get; set; }

    public decimal EstimatedCost { get; set; }

    public int EstimatedHours { get; set; }

    public int TargetEmployeeCount { get; set; }

    public TrainingActionType Category { get; set; }

    public string? Department { get; set; }

    public bool IsObligatory { get; set; }
}
