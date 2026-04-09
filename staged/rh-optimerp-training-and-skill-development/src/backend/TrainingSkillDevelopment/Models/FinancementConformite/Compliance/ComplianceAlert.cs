using Newtonsoft.Json;
using Training.SkillDevelopment.Models.Common;

namespace Training.SkillDevelopment.Models.FinancementConformite.Compliance;

public sealed class ComplianceAlert
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string ObligationId { get; set; } = string.Empty;

    public string? EmployeeId { get; set; }

    public RiskLevel RiskLevel { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTime DueDate { get; set; }

    public bool IsResolved { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
