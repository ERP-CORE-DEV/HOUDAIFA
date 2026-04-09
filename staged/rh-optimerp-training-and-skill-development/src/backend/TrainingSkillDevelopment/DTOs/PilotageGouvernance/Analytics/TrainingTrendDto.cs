namespace Training.SkillDevelopment.DTOs.PilotageGouvernance.Analytics;

public sealed class TrainingTrendDto
{
    public int Year { get; init; }

    public int TotalTrainingActions { get; init; }

    public decimal TotalBudget { get; init; }

    public decimal AverageSatisfaction { get; init; }

    public decimal CompletionRate { get; init; }
}
