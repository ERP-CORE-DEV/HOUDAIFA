using Newtonsoft.Json;
using Training.SkillDevelopment.Models.Auditing;
using Training.SkillDevelopment.Models.Common;

namespace Training.SkillDevelopment.Models.FormationExecution.TrainingPlan;

public sealed class TrainingPlan : IAuditable, ISoftDeletable
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string CompanyId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public int Year { get; set; }

    public TrainingPlanStatus Status { get; set; } = TrainingPlanStatus.Draft;

    public int Version { get; set; } = 1;

    public decimal BudgetAllocated { get; set; }

    public decimal BudgetConsumed { get; set; }

    public decimal MasseSalariale { get; set; }

    public decimal LegalObligationRate { get; set; } = 0.01m;

    public string? Description { get; set; }

    // IAuditable
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Approval
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }

    // ISoftDeletable
    public bool IsActive { get; set; } = true;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
