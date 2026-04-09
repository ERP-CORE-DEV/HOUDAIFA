using Newtonsoft.Json;
using Training.SkillDevelopment.Models.Common;

namespace Training.SkillDevelopment.Models.FormationExecution.TrainingAction;

public sealed class TrainingSession
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string ActionId { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string? Location { get; set; }

    public string? TrainerId { get; set; }

    public SessionStatus Status { get; set; } = SessionStatus.Planned;

    public int EnrolledCount { get; set; }

    public int MaxCapacity { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsActive { get; set; } = true;
}
