using Newtonsoft.Json;
using Training.SkillDevelopment.Models.Auditing;
using Training.SkillDevelopment.Models.Common;

namespace Training.SkillDevelopment.Models.FinancementConformite.Alternance;

public sealed class AlternanceContract : IAnonymizable
{
    public const int TrialPeriodDays = 45;
    public const int MinAge = 16;
    public const int MaxAge = 29;

    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string EmployeeId { get; set; } = string.Empty;

    public AlternanceContractType Type { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string? CfaId { get; set; }

    public string? TutorId { get; set; }

    public string? CertificationTargetId { get; set; }

    public decimal RemunerationPercentage { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsAnonymized { get; set; }

    public DateTime? AnonymizationDate { get; set; }

    public void AnonymizePersonalData()
    {
        EmployeeId = "ANONYMIZED";
        TutorId = null;
        IsAnonymized = true;
        AnonymizationDate = DateTime.UtcNow;
    }
}
