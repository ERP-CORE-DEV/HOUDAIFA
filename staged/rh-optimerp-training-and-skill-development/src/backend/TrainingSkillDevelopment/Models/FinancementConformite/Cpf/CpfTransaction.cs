using Newtonsoft.Json;
using Training.SkillDevelopment.Models.Common;

namespace Training.SkillDevelopment.Models.FinancementConformite.Cpf;

public sealed class CpfTransaction
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string AccountId { get; set; } = string.Empty;

    public CpfTransactionType Type { get; set; }

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public string? TrainingActionId { get; set; }

    public DateTime TransactionDate { get; set; }

    public string Source { get; set; } = string.Empty;
}
