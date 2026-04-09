using Newtonsoft.Json;
using Training.SkillDevelopment.Models.Auditing;
using Training.SkillDevelopment.Models.Common;

namespace Training.SkillDevelopment.Models.FinancementConformite.Compliance;

public sealed class EmployeeObligationStatus : IAnonymizable
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string EmployeeId { get; set; } = string.Empty;

    public string ObligationId { get; set; } = string.Empty;

    public ObligationStatus Status { get; set; }

    public DateTime? LastCompletedAt { get; set; }

    public DateTime? NextDueDate { get; set; }

    public string? TrainingActionId { get; set; }

    public DateTime UpdatedAt { get; set; }

    // IAnonymizable
    public bool IsAnonymized { get; set; }
    public DateTime? AnonymizationDate { get; set; }

    public void AnonymizePersonalData()
    {
        EmployeeId = "ANONYMIZED";
        IsAnonymized = true;
        AnonymizationDate = DateTime.UtcNow;
    }
}
