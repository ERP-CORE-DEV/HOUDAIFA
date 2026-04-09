using Newtonsoft.Json;

namespace Training.SkillDevelopment.Models.FormationExecution.TrainingPlan;

public sealed class TrainingBudget
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string PlanId { get; set; } = string.Empty;

    public decimal TotalBudget { get; set; }

    public decimal ObligatoryBudget { get; set; }

    public decimal DevelopmentBudget { get; set; }

    public decimal MasseSalariale { get; set; }

    public decimal LegalObligationRate { get; set; }

    public int Year { get; set; }

    public decimal ConsumedAmount { get; set; }

    public decimal RemainingBudget => TotalBudget - ConsumedAmount;

    public bool IsCompliant => TotalBudget >= MasseSalariale * LegalObligationRate;
}
