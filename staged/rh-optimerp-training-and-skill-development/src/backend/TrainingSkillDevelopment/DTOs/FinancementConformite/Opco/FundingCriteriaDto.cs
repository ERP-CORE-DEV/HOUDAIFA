using Training.SkillDevelopment.Models.FinancementConformite.Opco;

namespace Training.SkillDevelopment.DTOs.FinancementConformite.Opco;

public sealed class FundingCriteriaDto
{
    public string Id { get; set; } = string.Empty;

    public string OpcoId { get; set; } = string.Empty;

    public string TrainingType { get; set; } = string.Empty;

    public string CompanySizeRange { get; set; } = string.Empty;

    public decimal MaxHourlyRate { get; set; }

    public decimal MaxTotalAmount { get; set; }

    public string? EligibilityConditions { get; set; }

    public DateTime ValidFrom { get; set; }

    public DateTime ValidTo { get; set; }

    public static FundingCriteriaDto FromDomain(FundingCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        return new FundingCriteriaDto
        {
            Id = criteria.Id,
            OpcoId = criteria.OpcoId,
            TrainingType = criteria.TrainingType,
            CompanySizeRange = criteria.CompanySizeRange,
            MaxHourlyRate = criteria.MaxHourlyRate,
            MaxTotalAmount = criteria.MaxTotalAmount,
            EligibilityConditions = criteria.EligibilityConditions,
            ValidFrom = criteria.ValidFrom,
            ValidTo = criteria.ValidTo
        };
    }

    public FundingCriteria ToDomain()
    {
        return new FundingCriteria
        {
            Id = Id,
            OpcoId = OpcoId,
            TrainingType = TrainingType,
            CompanySizeRange = CompanySizeRange,
            MaxHourlyRate = MaxHourlyRate,
            MaxTotalAmount = MaxTotalAmount,
            EligibilityConditions = EligibilityConditions,
            ValidFrom = ValidFrom,
            ValidTo = ValidTo
        };
    }
}
