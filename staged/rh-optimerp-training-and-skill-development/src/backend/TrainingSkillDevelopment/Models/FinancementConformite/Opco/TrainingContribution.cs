using Newtonsoft.Json;

namespace Training.SkillDevelopment.Models.FinancementConformite.Opco;

public sealed class TrainingContribution
{
    public const decimal RateUnder11 = 0.0055m;
    public const decimal Rate11Plus = 0.01m;

    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string CompanyId { get; set; } = string.Empty;

    public int Year { get; set; }

    public decimal MasseSalariale { get; set; }

    public decimal ContributionRate { get; set; }

    public decimal TotalAmount { get; set; }
}
