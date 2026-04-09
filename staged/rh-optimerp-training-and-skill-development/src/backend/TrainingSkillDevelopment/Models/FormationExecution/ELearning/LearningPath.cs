using Newtonsoft.Json;

namespace Training.SkillDevelopment.Models.FormationExecution.ELearning;

public sealed class LearningPath
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string[] ModuleCourseIds { get; set; } = Array.Empty<string>();

    public string[]? Prerequisites { get; set; }

    public int EstimatedDurationHours { get; set; }

    public string? CertificationId { get; set; }

    public bool IsActive { get; set; } = true;
}
