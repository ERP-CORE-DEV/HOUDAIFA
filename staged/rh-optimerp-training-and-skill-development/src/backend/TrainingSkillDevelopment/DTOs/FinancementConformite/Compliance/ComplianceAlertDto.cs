using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FinancementConformite.Compliance;

namespace Training.SkillDevelopment.DTOs.FinancementConformite.Compliance;

public sealed class ComplianceAlertDto
{
    public string Id { get; set; } = string.Empty;

    public string ObligationId { get; set; } = string.Empty;

    public string? EmployeeId { get; set; }

    public RiskLevel RiskLevel { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTime DueDate { get; set; }

    public bool IsResolved { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public static ComplianceAlertDto FromDomain(ComplianceAlert alert)
    {
        ArgumentNullException.ThrowIfNull(alert);

        return new ComplianceAlertDto
        {
            Id = alert.Id,
            ObligationId = alert.ObligationId,
            EmployeeId = alert.EmployeeId,
            RiskLevel = alert.RiskLevel,
            Message = alert.Message,
            DueDate = alert.DueDate,
            IsResolved = alert.IsResolved,
            ResolvedAt = alert.ResolvedAt,
            CreatedAt = alert.CreatedAt
        };
    }
}
