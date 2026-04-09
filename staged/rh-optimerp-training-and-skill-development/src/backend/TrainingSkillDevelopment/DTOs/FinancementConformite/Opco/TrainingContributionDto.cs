using Training.SkillDevelopment.Models.FinancementConformite.Opco;

namespace Training.SkillDevelopment.DTOs.FinancementConformite.Opco;

public sealed class TrainingContributionDto
{
    public string Id { get; set; } = string.Empty;

    public string CompanyId { get; set; } = string.Empty;

    public int Year { get; set; }

    public decimal MasseSalariale { get; set; }

    public decimal ContributionRate { get; set; }

    public decimal TotalAmount { get; set; }

    public static TrainingContributionDto FromDomain(TrainingContribution contribution)
    {
        ArgumentNullException.ThrowIfNull(contribution);

        return new TrainingContributionDto
        {
            Id = contribution.Id,
            CompanyId = contribution.CompanyId,
            Year = contribution.Year,
            MasseSalariale = contribution.MasseSalariale,
            ContributionRate = contribution.ContributionRate,
            TotalAmount = contribution.TotalAmount
        };
    }

    public TrainingContribution ToDomain()
    {
        return new TrainingContribution
        {
            Id = Id,
            CompanyId = CompanyId,
            Year = Year,
            MasseSalariale = MasseSalariale,
            ContributionRate = ContributionRate,
            TotalAmount = TotalAmount
        };
    }
}
