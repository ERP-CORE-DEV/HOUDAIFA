using Newtonsoft.Json;
using Training.SkillDevelopment.Models.Auditing;
using Training.SkillDevelopment.Models.Common;

namespace Training.SkillDevelopment.Models.CertificationEcosysteme.Certification;

public sealed class VaeProject : IAnonymizable, IAuditable
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string EmployeeId { get; set; } = string.Empty;

    public string? CertificationId { get; set; }

    public string? RncpCode { get; set; }

    public VaeStatus Status { get; set; } = VaeStatus.Recevabilite;

    public DateTime StartDate { get; set; }

    public DateTime? ExpectedJuryDate { get; set; }

    public DateTime? JuryDate { get; set; }

    public string? AccompanimentOrganization { get; set; }

    public string? Notes { get; set; }

    // IAuditable
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // IAnonymizable
    public bool IsAnonymized { get; set; }
    public DateTime? AnonymizationDate { get; set; }

    public void AnonymizePersonalData()
    {
        EmployeeId = "ANONYMIZED";
        Notes = null;
        IsAnonymized = true;
        AnonymizationDate = DateTime.UtcNow;
    }
}
