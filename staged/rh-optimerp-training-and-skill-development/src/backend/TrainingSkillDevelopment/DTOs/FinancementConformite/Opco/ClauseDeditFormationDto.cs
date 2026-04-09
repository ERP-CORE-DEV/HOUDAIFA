using Training.SkillDevelopment.Models.FinancementConformite.Opco;

namespace Training.SkillDevelopment.DTOs.FinancementConformite.Opco;

public sealed class ClauseDeditFormationDto
{
    public string Id { get; set; } = string.Empty;

    public string EmployeeId { get; set; } = string.Empty;

    public string TrainingActionId { get; set; } = string.Empty;

    public decimal TotalCost { get; set; }

    public int ObligationDurationMonths { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal RemainingAmount { get; set; }

    public bool IsAnonymized { get; set; }

    public static ClauseDeditFormationDto FromDomain(ClauseDeditFormation clause)
    {
        ArgumentNullException.ThrowIfNull(clause);

        return new ClauseDeditFormationDto
        {
            Id = clause.Id,
            EmployeeId = clause.IsAnonymized ? "ANONYMIZED" : clause.EmployeeId,
            TrainingActionId = clause.TrainingActionId,
            TotalCost = clause.TotalCost,
            ObligationDurationMonths = clause.ObligationDurationMonths,
            StartDate = clause.StartDate,
            EndDate = clause.EndDate,
            RemainingAmount = clause.RemainingAmount,
            IsAnonymized = clause.IsAnonymized
        };
    }

    public ClauseDeditFormation ToDomain()
    {
        return new ClauseDeditFormation
        {
            Id = Id,
            EmployeeId = EmployeeId,
            TrainingActionId = TrainingActionId,
            TotalCost = TotalCost,
            ObligationDurationMonths = ObligationDurationMonths,
            StartDate = StartDate,
            EndDate = EndDate,
            RemainingAmount = RemainingAmount,
            IsAnonymized = IsAnonymized
        };
    }
}
