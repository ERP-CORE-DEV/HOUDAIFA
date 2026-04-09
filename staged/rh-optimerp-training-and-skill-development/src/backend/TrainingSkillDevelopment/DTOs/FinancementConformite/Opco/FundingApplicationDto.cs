using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FinancementConformite.Opco;

namespace Training.SkillDevelopment.DTOs.FinancementConformite.Opco;

public sealed class FundingApplicationDto
{
    public string Id { get; set; } = string.Empty;

    public string OpcoId { get; set; } = string.Empty;

    public string TrainingActionId { get; set; } = string.Empty;

    public string[] EmployeeIds { get; set; } = Array.Empty<string>();

    public decimal RequestedAmount { get; set; }

    public decimal? GrantedAmount { get; set; }

    public FundingStatus Status { get; set; }

    public DateTime SubmissionDate { get; set; }

    public DateTime? DecisionDate { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public static FundingApplicationDto FromDomain(FundingApplication application)
    {
        ArgumentNullException.ThrowIfNull(application);

        return new FundingApplicationDto
        {
            Id = application.Id,
            OpcoId = application.OpcoId,
            TrainingActionId = application.TrainingActionId,
            EmployeeIds = application.EmployeeIds,
            RequestedAmount = application.RequestedAmount,
            GrantedAmount = application.GrantedAmount,
            Status = application.Status,
            SubmissionDate = application.SubmissionDate,
            DecisionDate = application.DecisionDate,
            RejectionReason = application.RejectionReason,
            CreatedAt = application.CreatedAt,
            UpdatedAt = application.UpdatedAt
        };
    }

    public FundingApplication ToDomain()
    {
        return new FundingApplication
        {
            Id = Id,
            OpcoId = OpcoId,
            TrainingActionId = TrainingActionId,
            EmployeeIds = EmployeeIds,
            RequestedAmount = RequestedAmount,
            GrantedAmount = GrantedAmount,
            Status = Status,
            SubmissionDate = SubmissionDate,
            DecisionDate = DecisionDate,
            RejectionReason = RejectionReason,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };
    }
}
