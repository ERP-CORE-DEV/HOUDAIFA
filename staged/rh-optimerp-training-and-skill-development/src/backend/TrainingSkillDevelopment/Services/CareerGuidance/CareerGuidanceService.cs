using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.DTOs.CareerGuidance;
using Training.SkillDevelopment.Repositories.Competency;
using Training.SkillDevelopment.Repositories.FormationExecution.TrainingAction;

namespace Training.SkillDevelopment.Services.CareerGuidance;

public sealed class CareerGuidanceService : ICareerGuidanceService
{
    private readonly ICompetencyRepository _competencyRepository;
    private readonly ICompetencyAssessmentRepository _assessmentRepository;
    private readonly ITrainingActionRepository _trainingActionRepository;
    private readonly ILogger<CareerGuidanceService> _logger;

    public CareerGuidanceService(
        ICompetencyRepository competencyRepository,
        ICompetencyAssessmentRepository assessmentRepository,
        ITrainingActionRepository trainingActionRepository,
        ILogger<CareerGuidanceService> logger)
    {
        ArgumentNullException.ThrowIfNull(competencyRepository);
        ArgumentNullException.ThrowIfNull(assessmentRepository);
        ArgumentNullException.ThrowIfNull(trainingActionRepository);
        ArgumentNullException.ThrowIfNull(logger);

        _competencyRepository = competencyRepository;
        _assessmentRepository = assessmentRepository;
        _trainingActionRepository = trainingActionRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TrainingRecommendationDto>> GetTrainingRecommendationsAsync(string employeeId)
    {
        if (string.IsNullOrWhiteSpace(employeeId))
            throw new ArgumentException("L'identifiant de l'employe est obligatoire.", nameof(employeeId));

        _logger.LogInformation(
            "Calcul des recommandations de formation pour l'employe {EmployeeId}.", employeeId);

        var gaps = await _assessmentRepository.GetGapsAsync(employeeId);
        var prioritizedGaps = gaps
            .Where(g => g.GapSize > 0)
            .OrderByDescending(g => g.GapSize)
            .ThenByDescending(g => g.Priority)
            .ToList();

        if (prioritizedGaps.Count == 0)
        {
            _logger.LogInformation(
                "Aucun ecart de competence detecte pour l'employe {EmployeeId}.", employeeId);
            return Array.Empty<TrainingRecommendationDto>();
        }

        var allActions = await _trainingActionRepository.GetAllAsync();
        var recommendations = new List<TrainingRecommendationDto>();

        foreach (var gap in prioritizedGaps)
        {
            var competency = await _competencyRepository.GetByIdAsync(gap.CompetencyId);
            if (competency is null)
                continue;

            var matchingAction = allActions.FirstOrDefault(a =>
                a.IsActive &&
                a.CompetencyIds != null &&
                a.CompetencyIds.Contains(gap.CompetencyId));

            var recommendedActionId = gap.RecommendedTrainingActionId
                ?? matchingAction?.Id
                ?? string.Empty;

            var recommendedActionTitle = matchingAction?.Title ?? string.Empty;

            if (!string.IsNullOrEmpty(gap.RecommendedTrainingActionId) && matchingAction is null)
            {
                var specificAction = await _trainingActionRepository.GetByIdAsync(gap.RecommendedTrainingActionId);
                if (specificAction is not null)
                    recommendedActionTitle = specificAction.Title;
            }

            recommendations.Add(new TrainingRecommendationDto
            {
                EmployeeId = employeeId,
                CompetencyName = competency.Name,
                CurrentLevel = (int)gap.CurrentLevel,
                RequiredLevel = (int)gap.RequiredLevel,
                GapSize = gap.GapSize,
                RecommendedTrainingActionId = recommendedActionId,
                RecommendedTrainingActionTitle = recommendedActionTitle,
                Priority = gap.Priority
            });
        }

        _logger.LogInformation(
            "{Count} recommandation(s) generee(s) pour l'employe {EmployeeId}.",
            recommendations.Count, employeeId);

        return recommendations.AsReadOnly();
    }

    public async Task<IReadOnlyList<JobTransitionPathDto>> GetJobTransitionPathsAsync(string currentRoleId)
    {
        if (string.IsNullOrWhiteSpace(currentRoleId))
            throw new ArgumentException("L'identifiant du poste actuel est obligatoire.", nameof(currentRoleId));

        _logger.LogInformation(
            "Calcul des parcours de transition professionnelle depuis le poste {RoleId}.", currentRoleId);

        var criticalCompetencies = await _competencyRepository.GetByCriticalAsync();
        var allActions = await _trainingActionRepository.GetAllAsync();

        if (criticalCompetencies.Count == 0)
        {
            _logger.LogInformation(
                "Aucune competence critique definie pour le poste {RoleId}.", currentRoleId);
            return Array.Empty<JobTransitionPathDto>();
        }

        var actionsByDomain = allActions
            .Where(a => a.IsActive)
            .GroupBy(a => a.Category ?? "General")
            .ToList();

        var paths = new List<JobTransitionPathDto>();

        foreach (var domainGroup in actionsByDomain)
        {
            var targetRoleId = $"ROLE-{domainGroup.Key.ToUpperInvariant().Replace(" ", "-")}";

            if (targetRoleId == currentRoleId)
                continue;

            var requiredCompetencies = criticalCompetencies
                .Where(c => c.Domain == domainGroup.Key || c.Family == domainGroup.Key)
                .ToList();

            var gapCount = requiredCompetencies.Count;
            var matchPercentage = gapCount == 0
                ? 100m
                : Math.Round((decimal)(criticalCompetencies.Count - gapCount) / criticalCompetencies.Count * 100, 2);

            var requiredTrainings = domainGroup
                .Take(3)
                .Select(a => a.Title)
                .ToArray();

            paths.Add(new JobTransitionPathDto
            {
                CurrentRoleId = currentRoleId,
                TargetRoleId = targetRoleId,
                TargetRoleName = domainGroup.Key,
                MatchPercentage = matchPercentage,
                GapCount = gapCount,
                RequiredTrainings = requiredTrainings
            });
        }

        var orderedPaths = paths
            .OrderByDescending(p => p.MatchPercentage)
            .ThenBy(p => p.GapCount)
            .ToList();

        _logger.LogInformation(
            "{Count} parcours de transition disponibles depuis le poste {RoleId}.",
            orderedPaths.Count, currentRoleId);

        return orderedPaths.AsReadOnly();
    }
}
