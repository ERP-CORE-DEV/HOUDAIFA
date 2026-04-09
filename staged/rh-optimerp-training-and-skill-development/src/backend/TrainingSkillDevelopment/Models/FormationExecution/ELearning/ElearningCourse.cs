using Newtonsoft.Json;

namespace Training.SkillDevelopment.Models.FormationExecution.ELearning;

public sealed class ElearningCourse
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>Valeurs acceptees : SCORM, Video, Interactive, MicroLearning.</summary>
    public string Format { get; set; } = string.Empty;

    public string? ScormPackageId { get; set; }

    public int DurationMinutes { get; set; }

    public string? Provider { get; set; }

    public string[]? Tags { get; set; }

    public string[]? CompetencyIds { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
