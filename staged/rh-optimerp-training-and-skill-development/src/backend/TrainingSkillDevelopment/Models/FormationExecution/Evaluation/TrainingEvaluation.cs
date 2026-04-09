using Newtonsoft.Json;
using Training.SkillDevelopment.Models.Auditing;
using Training.SkillDevelopment.Models.Common;

namespace Training.SkillDevelopment.Models.FormationExecution.Evaluation;

public sealed class TrainingEvaluation : IAnonymizable, IAuditable
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string SessionId { get; set; } = string.Empty;

    public string EmployeeId { get; set; } = string.Empty;

    public EvaluationLevel Level { get; set; }

    public decimal Score { get; set; }

    public int MaxScore { get; set; } = 10;

    public string? Comments { get; set; }

    public DateTime EvaluatedAt { get; set; }

    public string? EvaluatorId { get; set; }

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
        Comments = null;
        EvaluatorId = null;
        IsAnonymized = true;
        AnonymizationDate = DateTime.UtcNow;
    }
}
