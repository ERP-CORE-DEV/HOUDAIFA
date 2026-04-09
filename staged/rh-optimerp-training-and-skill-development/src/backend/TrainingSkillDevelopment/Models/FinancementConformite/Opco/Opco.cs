using Newtonsoft.Json;

namespace Training.SkillDevelopment.Models.FinancementConformite.Opco;

public sealed class Opco
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Website { get; set; }

    public string? ContactInfo { get; set; }

    public string[]? ConventionCollectiveIds { get; set; }

    public bool IsActive { get; set; } = true;
}
