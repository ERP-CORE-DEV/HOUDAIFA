using Newtonsoft.Json;
using Training.SkillDevelopment.Models.Auditing;
using Training.SkillDevelopment.Models.Common;

namespace Training.SkillDevelopment.Models.FinancementConformite.Compliance;

public sealed class TrainingObligation : IAuditable, ISoftDeletable
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// Reference reglementaire (ex: "L6321-1 Code du travail").
    /// </summary>
    public string? RegulatoryReference { get; set; }

    public string? TargetJobFamily { get; set; }

    public int FrequencyMonths { get; set; }

    public RiskLevel RiskLevel { get; set; }

    public ObligationStatus Status { get; set; } = ObligationStatus.Current;

    public DateTime EffectiveDate { get; set; }

    public DateTime? ExpirationDate { get; set; }

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
