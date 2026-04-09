namespace Training.SkillDevelopment.DTOs.PilotageGouvernance.Analytics;

public sealed class TrainingDashboardKpiDto
{
    public int Year { get; init; }

    public int TotalTrainingPlans { get; init; }

    public int TotalTrainingActions { get; init; }

    public int TotalSessions { get; init; }

    public int TotalEnrollments { get; init; }

    public decimal TotalBudgetAllocated { get; init; }

    public decimal TotalBudgetConsumed { get; init; }

    public decimal BudgetUtilizationRate { get; init; }

    public decimal AverageSatisfactionScore { get; init; }

    public decimal CompletionRate { get; init; }

    public decimal CpfMobilizationRate { get; init; }
}
