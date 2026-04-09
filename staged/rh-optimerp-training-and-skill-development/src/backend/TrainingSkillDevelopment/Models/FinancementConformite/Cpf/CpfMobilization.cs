using Newtonsoft.Json;

namespace Training.SkillDevelopment.Models.FinancementConformite.Cpf;

public sealed class CpfMobilization
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string AccountId { get; set; } = string.Empty;

    public string TrainingActionId { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public bool DuringWorkingHours { get; set; }

    public bool? EmployerApproval { get; set; }

    public decimal ResteACharge { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime RequestDate { get; set; }

    public DateTime? ApprovalDate { get; set; }
}
