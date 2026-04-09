using Newtonsoft.Json;
using Training.SkillDevelopment.Models.Auditing;

namespace Training.SkillDevelopment.Models.CertificationEcosysteme.Certification;

public sealed class RegulatoryHabilitation : IAuditable, ISoftDeletable
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string EmployeeId { get; set; } = string.Empty;

    public string HabilitationCode { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? RegulatoryReference { get; set; }

    public DateTime ObtainedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public string? IssuingAuthority { get; set; }

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
