namespace Training.SkillDevelopment.DTOs.PilotageGouvernance.Analytics;

public sealed class GenderEqualityTrainingReportDto
{
    public int Year { get; init; }

    public decimal FemaleTrainingHours { get; init; }

    public decimal MaleTrainingHours { get; init; }

    public decimal FemaleTrainingRate { get; init; }

    public decimal MaleTrainingRate { get; init; }

    public decimal GapPercentage { get; init; }
}
