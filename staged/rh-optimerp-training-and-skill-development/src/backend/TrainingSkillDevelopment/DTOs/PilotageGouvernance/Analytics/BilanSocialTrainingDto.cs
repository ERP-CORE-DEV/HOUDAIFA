namespace Training.SkillDevelopment.DTOs.PilotageGouvernance.Analytics;

public sealed class BilanSocialTrainingDto
{
    public int Year { get; init; }

    public decimal TotalTrainingHours { get; init; }

    public decimal TotalTrainingCost { get; init; }

    public decimal TrainingCostPerEmployee { get; init; }

    public decimal PercentageOfMasseSalariale { get; init; }

    public Dictionary<string, int> TrainingsByCategory { get; init; } = new();

    public Dictionary<string, int> TrainingsByGender { get; init; } = new();
}
