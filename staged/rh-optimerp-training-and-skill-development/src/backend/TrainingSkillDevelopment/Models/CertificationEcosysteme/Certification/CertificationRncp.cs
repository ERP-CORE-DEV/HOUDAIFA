using Newtonsoft.Json;
using Training.SkillDevelopment.Models.Auditing;

namespace Training.SkillDevelopment.Models.CertificationEcosysteme.Certification;

public sealed class CertificationRncp : IAuditable, ISoftDeletable
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Code RNCP ou RS au format RNCPXXXXX ou RSXXXXX.
    /// </summary>
    public string RncpCode { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? CertifyingBody { get; set; }

    public int? NsfCode { get; set; }

    public string? Level { get; set; }

    public DateTime? RegistrationDate { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public string[]? EligibleOpcoIds { get; set; }

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
