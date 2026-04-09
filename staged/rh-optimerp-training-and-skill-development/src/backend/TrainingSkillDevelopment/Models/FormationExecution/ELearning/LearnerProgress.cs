using Newtonsoft.Json;
using Training.SkillDevelopment.Models.Auditing;

namespace Training.SkillDevelopment.Models.FormationExecution.ELearning;

public sealed class LearnerProgress : IAnonymizable
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string EmployeeId { get; set; } = string.Empty;

    public string CourseId { get; set; } = string.Empty;

    /// <summary>Valeurs acceptees : NotStarted, InProgress, Completed.</summary>
    public string Status { get; set; } = "NotStarted";

    public int ProgressPercentage { get; set; }

    public int TimeSpentMinutes { get; set; }

    public int? Score { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public bool IsAnonymized { get; set; }

    public DateTime? AnonymizationDate { get; set; }

    public void AnonymizePersonalData()
    {
        EmployeeId = "ANONYMIZED";
        IsAnonymized = true;
        AnonymizationDate = DateTime.UtcNow;
    }
}
