using Newtonsoft.Json;
using Training.SkillDevelopment.Models.Auditing;

namespace Training.SkillDevelopment.Models.FinancementConformite.Cpf;

public sealed class CpfAccount : IAuditable, IAnonymizable
{
    public const decimal StandardAnnualCredit = 500m;
    public const decimal LowQualifiedAnnualCredit = 800m;
    public const decimal StandardCeiling = 5000m;
    public const decimal LowQualifiedCeiling = 8000m;
    public const decimal ResteAChargeForfaitaire = 100m;

    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string EmployeeId { get; set; } = string.Empty;

    public decimal BalanceEuros { get; set; }

    public decimal CeilingEuros { get; set; } = StandardCeiling;

    public decimal AnnualCreditEuros { get; set; } = StandardAnnualCredit;

    public bool IsLowQualified { get; set; }

    public DateTime? LastCreditDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public bool IsAnonymized { get; set; }

    public DateTime? AnonymizationDate { get; set; }

    public void AnonymizePersonalData()
    {
        EmployeeId = "ANONYMIZED";
        IsAnonymized = true;
        AnonymizationDate = DateTime.UtcNow;
    }
}
