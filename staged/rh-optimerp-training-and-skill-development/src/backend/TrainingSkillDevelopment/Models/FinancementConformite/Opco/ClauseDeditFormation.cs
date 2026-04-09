using Newtonsoft.Json;
using Training.SkillDevelopment.Models.Auditing;

namespace Training.SkillDevelopment.Models.FinancementConformite.Opco;

public sealed class ClauseDeditFormation : IAnonymizable
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string EmployeeId { get; set; } = string.Empty;

    public string TrainingActionId { get; set; } = string.Empty;

    public decimal TotalCost { get; set; }

    public int ObligationDurationMonths { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal RemainingAmount { get; set; }

    public bool IsAnonymized { get; set; }

    public DateTime? AnonymizationDate { get; set; }

    public void AnonymizePersonalData()
    {
        EmployeeId = "ANONYMIZED";
        IsAnonymized = true;
        AnonymizationDate = DateTime.UtcNow;
    }
}
