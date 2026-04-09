using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.DTOs.PilotageGouvernance.Analytics;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Repositories.FinancementConformite.Cpf;
using Training.SkillDevelopment.Repositories.FormationExecution.Evaluation;
using Training.SkillDevelopment.Repositories.FormationExecution.TrainingAction;
using Training.SkillDevelopment.Repositories.FormationExecution.TrainingPlan;

namespace Training.SkillDevelopment.Services.PilotageGouvernance.Analytics;

public sealed class TrainingAnalyticsService : ITrainingAnalyticsService
{
    private readonly ITrainingPlanRepository _planRepository;
    private readonly ITrainingActionRepository _actionRepository;
    private readonly ITrainingSessionRepository _sessionRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ITrainingEvaluationRepository _evaluationRepository;
    private readonly ICpfAccountRepository _cpfAccountRepository;
    private readonly ILogger<TrainingAnalyticsService> _logger;

    public TrainingAnalyticsService(
        ITrainingPlanRepository planRepository,
        ITrainingActionRepository actionRepository,
        ITrainingSessionRepository sessionRepository,
        IEnrollmentRepository enrollmentRepository,
        ITrainingEvaluationRepository evaluationRepository,
        ICpfAccountRepository cpfAccountRepository,
        ILogger<TrainingAnalyticsService> logger)
    {
        ArgumentNullException.ThrowIfNull(planRepository);
        ArgumentNullException.ThrowIfNull(actionRepository);
        ArgumentNullException.ThrowIfNull(sessionRepository);
        ArgumentNullException.ThrowIfNull(enrollmentRepository);
        ArgumentNullException.ThrowIfNull(evaluationRepository);
        ArgumentNullException.ThrowIfNull(cpfAccountRepository);
        ArgumentNullException.ThrowIfNull(logger);

        _planRepository = planRepository;
        _actionRepository = actionRepository;
        _sessionRepository = sessionRepository;
        _enrollmentRepository = enrollmentRepository;
        _evaluationRepository = evaluationRepository;
        _cpfAccountRepository = cpfAccountRepository;
        _logger = logger;
    }

    public async Task<TrainingDashboardKpiDto> GetDashboardKpisAsync(int year)
    {
        if (year < 2000 || year > 2100)
            throw new ArgumentOutOfRangeException(nameof(year), "L'annee doit etre comprise entre 2000 et 2100.");

        _logger.LogInformation("Calcul des KPIs du tableau de bord pour l'annee {Year}.", year);

        var plans = await _planRepository.GetByYearAsync(year);
        var allActions = await _actionRepository.GetAllAsync();

        var yearStart = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearEnd = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        var sessions = await _sessionRepository.GetByDateRangeAsync(yearStart, yearEnd);

        var totalBudgetAllocated = plans.Sum(p => p.BudgetAllocated);
        var totalBudgetConsumed = plans.Sum(p => p.BudgetConsumed);

        var totalEnrollments = 0;
        var completedEnrollments = 0;
        var allSatisfactionScores = new List<decimal>();

        foreach (var session in sessions)
        {
            var sessionEnrollments = await _enrollmentRepository.GetBySessionIdAsync(session.Id);
            totalEnrollments += sessionEnrollments.Count;
            completedEnrollments += sessionEnrollments.Count(e => e.Status == EnrollmentStatus.Attended);

            if (session.Status == SessionStatus.Completed)
            {
                var score = await _evaluationRepository.GetAverageScoreAsync(session.Id);
                if (score > 0)
                    allSatisfactionScores.Add(score);
            }
        }

        var completionRate = totalEnrollments > 0
            ? Math.Round((decimal)completedEnrollments / totalEnrollments * 100, 2)
            : 0m;

        var averageSatisfaction = allSatisfactionScores.Count > 0
            ? Math.Round(allSatisfactionScores.Average(), 2)
            : 0m;

        var budgetUtilizationRate = totalBudgetAllocated > 0
            ? Math.Round(totalBudgetConsumed / totalBudgetAllocated * 100, 2)
            : 0m;

        return new TrainingDashboardKpiDto
        {
            Year = year,
            TotalTrainingPlans = plans.Count,
            TotalTrainingActions = allActions.Count(a => a.IsActive),
            TotalSessions = sessions.Count,
            TotalEnrollments = totalEnrollments,
            TotalBudgetAllocated = totalBudgetAllocated,
            TotalBudgetConsumed = totalBudgetConsumed,
            BudgetUtilizationRate = budgetUtilizationRate,
            AverageSatisfactionScore = averageSatisfaction,
            CompletionRate = completionRate,
            CpfMobilizationRate = 0m
        };
    }

    public async Task<BilanSocialTrainingDto> GetBilanSocialTrainingAsync(int year)
    {
        if (year < 2000 || year > 2100)
            throw new ArgumentOutOfRangeException(nameof(year), "L'annee doit etre comprise entre 2000 et 2100.");

        _logger.LogInformation("Calcul du bilan social formation pour l'annee {Year}.", year);

        var plans = await _planRepository.GetByYearAsync(year);
        var allActions = await _actionRepository.GetAllAsync();

        var totalTrainingCost = plans.Sum(p => p.BudgetConsumed);
        var masseSalariale = plans.Sum(p => p.MasseSalariale);

        var yearStart = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearEnd = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        var sessions = await _sessionRepository.GetByDateRangeAsync(yearStart, yearEnd);

        var totalEnrollments = 0;
        foreach (var session in sessions)
        {
            var enrollments = await _enrollmentRepository.GetBySessionIdAsync(session.Id);
            totalEnrollments += enrollments.Count(e => e.Status == EnrollmentStatus.Attended);
        }

        var totalTrainingHours = allActions
            .Sum(a => (decimal)a.DurationHours);

        var trainingsByCategory = allActions
            .Where(a => a.IsActive && !string.IsNullOrEmpty(a.Category))
            .GroupBy(a => a.Category!)
            .ToDictionary(g => g.Key, g => g.Count());

        var percentageOfMasseSalariale = masseSalariale > 0
            ? Math.Round(totalTrainingCost / masseSalariale * 100, 2)
            : 0m;

        var trainingCostPerEmployee = totalEnrollments > 0
            ? Math.Round(totalTrainingCost / totalEnrollments, 2)
            : 0m;

        return new BilanSocialTrainingDto
        {
            Year = year,
            TotalTrainingHours = totalTrainingHours,
            TotalTrainingCost = totalTrainingCost,
            TrainingCostPerEmployee = trainingCostPerEmployee,
            PercentageOfMasseSalariale = percentageOfMasseSalariale,
            TrainingsByCategory = trainingsByCategory,
            TrainingsByGender = new Dictionary<string, int>
            {
                { "Femme", 0 },
                { "Homme", 0 }
            }
        };
    }

    public async Task<GenderEqualityTrainingReportDto> GetGenderEqualityReportAsync(int year)
    {
        if (year < 2000 || year > 2100)
            throw new ArgumentOutOfRangeException(nameof(year), "L'annee doit etre comprise entre 2000 et 2100.");

        _logger.LogInformation("Calcul du rapport egalite femmes-hommes formation pour l'annee {Year}.", year);

        var yearStart = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearEnd = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        var sessions = await _sessionRepository.GetByDateRangeAsync(yearStart, yearEnd);

        var totalEnrolled = 0;
        foreach (var session in sessions)
        {
            var enrollments = await _enrollmentRepository.GetBySessionIdAsync(session.Id);
            totalEnrolled += enrollments.Count(e => e.Status == EnrollmentStatus.Attended);
        }

        var femaleHours = 0m;
        var maleHours = 0m;
        var femaleCount = 0;
        var maleCount = 0;

        var femaleRate = femaleCount + maleCount > 0
            ? Math.Round((decimal)femaleCount / (femaleCount + maleCount) * 100, 2)
            : 0m;
        var maleRate = femaleCount + maleCount > 0
            ? Math.Round((decimal)maleCount / (femaleCount + maleCount) * 100, 2)
            : 0m;
        var gapPercentage = Math.Abs(femaleRate - maleRate);

        return new GenderEqualityTrainingReportDto
        {
            Year = year,
            FemaleTrainingHours = femaleHours,
            MaleTrainingHours = maleHours,
            FemaleTrainingRate = femaleRate,
            MaleTrainingRate = maleRate,
            GapPercentage = gapPercentage
        };
    }

    public async Task<decimal> CalculateTrainingRoiAsync(string trainingActionId)
    {
        if (string.IsNullOrWhiteSpace(trainingActionId))
            throw new ArgumentException("L'identifiant de l'action de formation est obligatoire.", nameof(trainingActionId));

        _logger.LogInformation("Calcul du ROI pour l'action de formation {ActionId}.", trainingActionId);

        var action = await _actionRepository.GetByIdAsync(trainingActionId)
            ?? throw new KeyNotFoundException($"L'action de formation '{trainingActionId}' est introuvable.");

        var sessions = await _sessionRepository.GetByActionIdAsync(trainingActionId);

        var level4Scores = new List<decimal>();
        foreach (var session in sessions)
        {
            var evaluations = await _evaluationRepository.GetBySessionIdAsync(session.Id);
            var resultatsEvaluations = evaluations
                .Where(e => e.Level == Models.Common.EvaluationLevel.Resultats && e.MaxScore > 0);

            foreach (var eval in resultatsEvaluations)
            {
                level4Scores.Add(eval.Score / eval.MaxScore * 100);
            }
        }

        if (level4Scores.Count == 0 || action.Cost <= 0)
            return 0m;

        var averageLevel4Score = level4Scores.Average();
        var roi = Math.Round(averageLevel4Score / action.Cost * 100, 2);

        _logger.LogInformation(
            "ROI calcule pour l'action {ActionId}: {Roi}% (score moyen niveau 4: {Score}, cout: {Cost}).",
            trainingActionId, roi, averageLevel4Score, action.Cost);

        return roi;
    }

    public async Task<IReadOnlyList<TrainingTrendDto>> GetTrainingTrendsAsync(int startYear, int endYear)
    {
        if (startYear > endYear)
            throw new ArgumentException("L'annee de debut doit etre inferieure ou egale a l'annee de fin.");

        if (endYear - startYear > 20)
            throw new ArgumentException("La plage d'annees ne peut pas depasser 20 ans.");

        _logger.LogInformation(
            "Calcul des tendances de formation de {StartYear} a {EndYear}.", startYear, endYear);

        var trends = new List<TrainingTrendDto>();

        for (var year = startYear; year <= endYear; year++)
        {
            var plans = await _planRepository.GetByYearAsync(year);
            var actions = await _actionRepository.GetAllAsync();
            var yearStart = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var yearEnd = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);
            var sessions = await _sessionRepository.GetByDateRangeAsync(yearStart, yearEnd);

            var totalBudget = plans.Sum(p => p.BudgetConsumed);
            var totalEnrollments = 0;
            var completedEnrollments = 0;
            var satisfactionScores = new List<decimal>();

            foreach (var session in sessions)
            {
                var enrollments = await _enrollmentRepository.GetBySessionIdAsync(session.Id);
                totalEnrollments += enrollments.Count;
                completedEnrollments += enrollments.Count(e => e.Status == EnrollmentStatus.Attended);

                if (session.Status == SessionStatus.Completed)
                {
                    var score = await _evaluationRepository.GetAverageScoreAsync(session.Id);
                    if (score > 0)
                        satisfactionScores.Add(score);
                }
            }

            var completionRate = totalEnrollments > 0
                ? Math.Round((decimal)completedEnrollments / totalEnrollments * 100, 2)
                : 0m;

            var averageSatisfaction = satisfactionScores.Count > 0
                ? Math.Round(satisfactionScores.Average(), 2)
                : 0m;

            trends.Add(new TrainingTrendDto
            {
                Year = year,
                TotalTrainingActions = actions.Count(a => a.IsActive),
                TotalBudget = totalBudget,
                AverageSatisfaction = averageSatisfaction,
                CompletionRate = completionRate
            });
        }

        return trends.AsReadOnly();
    }
}
