using Newtonsoft.Json;
using Training.SkillDevelopment.Models.Auditing;

namespace Training.SkillDevelopment.Models.CertificationEcosysteme.Certification;

public sealed class EmployeeCertification : IAnonymizable, IAuditable
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string EmployeeId { get; set; } = string.Empty;

    public string CertificationId { get; set; } = string.Empty;

    public string? RncpCode { get; set; }

    public DateTime ObtainedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public string? CertifyingBody { get; set; }

    public string? CertificateNumber { get; set; }

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
        CertificateNumber = null;
        IsAnonymized = true;
        AnonymizationDate = DateTime.UtcNow;
    }
}
