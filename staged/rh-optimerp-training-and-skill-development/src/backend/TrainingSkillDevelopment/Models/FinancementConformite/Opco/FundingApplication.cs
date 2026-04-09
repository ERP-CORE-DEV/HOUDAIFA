using Newtonsoft.Json;
using Training.SkillDevelopment.Models.Common;

namespace Training.SkillDevelopment.Models.FinancementConformite.Opco;

public sealed class FundingApplication
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string OpcoId { get; set; } = string.Empty;

    public string TrainingActionId { get; set; } = string.Empty;

    public string[] EmployeeIds { get; set; } = Array.Empty<string>();

    public decimal RequestedAmount { get; set; }

    public decimal? GrantedAmount { get; set; }

    public FundingStatus Status { get; set; } = FundingStatus.Draft;

    public DateTime SubmissionDate { get; set; }

    public DateTime? DecisionDate { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
