using Newtonsoft.Json;

namespace Training.SkillDevelopment.Models.FinancementConformite.Opco;

public sealed class FundingCriteria
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string OpcoId { get; set; } = string.Empty;

    public string TrainingType { get; set; } = string.Empty;

    public string CompanySizeRange { get; set; } = string.Empty;

    public decimal MaxHourlyRate { get; set; }

    public decimal MaxTotalAmount { get; set; }

    public string? EligibilityConditions { get; set; }

    public DateTime ValidFrom { get; set; }

    public DateTime ValidTo { get; set; }
}
