using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FormationExecution.Evaluation;
using Training.SkillDevelopment.Repositories.FormationExecution.Evaluation;

namespace Training.SkillDevelopment.Services.FormationExecution.Evaluation;

public sealed class TrainingEvaluationService : ITrainingEvaluationService
{
    private readonly ITrainingEvaluationRepository _repository;
    private readonly ILogger<TrainingEvaluationService> _logger;

    public TrainingEvaluationService(
        ITrainingEvaluationRepository repository,
        ILogger<TrainingEvaluationService> logger)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(logger);
        _repository = repository;
        _logger = logger;
    }

    public async Task<TrainingEvaluation?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant de l'evaluation est obligatoire.", nameof(id));

        return await _repository.GetByIdAsync(id);
    }

    public async Task<IReadOnlyList<TrainingEvaluation>> GetBySessionIdAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("L'identifiant de la session est obligatoire.", nameof(sessionId));

        return await _repository.GetBySessionIdAsync(sessionId);
    }

    public async Task<IReadOnlyList<TrainingEvaluation>> GetByEmployeeIdAsync(string employeeId)
    {
        if (string.IsNullOrWhiteSpace(employeeId))
            throw new ArgumentException("L'identifiant de l'employe est obligatoire.", nameof(employeeId));

        return await _repository.GetByEmployeeIdAsync(employeeId);
    }

    public async Task<decimal> GetAverageScoreAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("L'identifiant de la session est obligatoire.", nameof(sessionId));

        var average = await _repository.GetAverageScoreAsync(sessionId);

        _logger.LogInformation(
            "Score moyen de la session {SessionId} (modele Kirkpatrick): {Average:F2}.",
            sessionId, average);

        return average;
    }

    public async Task<decimal> CalculateRoiAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("L'identifiant de la session est obligatoire.", nameof(sessionId));

        var evaluations = await _repository.GetBySessionIdAsync(sessionId);

        var resultLevelEvaluations = evaluations
            .Where(e => e.Level == EvaluationLevel.Resultats)
            .ToList();

        if (resultLevelEvaluations.Count == 0)
        {
            _logger.LogInformation(
                "Aucune evaluation niveau 4 (Resultats) disponible pour la session {SessionId}.",
                sessionId);
            return 0m;
        }

        var averageResultScore = resultLevelEvaluations.Average(e => e.Score / e.MaxScore);
        var roiPercentage = Math.Round(averageResultScore * 100m, 2);

        _logger.LogInformation(
            "ROI calcule pour la session {SessionId} (Kirkpatrick niveau 4): {Roi}%.",
            sessionId, roiPercentage);

        return roiPercentage;
    }

    public async Task<TrainingEvaluation> CreateAsync(TrainingEvaluation evaluation)
    {
        ArgumentNullException.ThrowIfNull(evaluation);
        ValidateEvaluation(evaluation);

        evaluation.Id = Guid.NewGuid().ToString();
        evaluation.EvaluatedAt = DateTime.UtcNow;
        evaluation.CreatedAt = DateTime.UtcNow;
        evaluation.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Creation d'une evaluation Kirkpatrick niveau {Level} pour la session {SessionId}.",
            evaluation.Level, evaluation.SessionId);

        return await _repository.AddAsync(evaluation);
    }

    public async Task<TrainingEvaluation> UpdateAsync(TrainingEvaluation evaluation)
    {
        ArgumentNullException.ThrowIfNull(evaluation);

        var existing = await _repository.GetByIdAsync(evaluation.Id)
            ?? throw new KeyNotFoundException($"L'evaluation '{evaluation.Id}' est introuvable.");

        ValidateEvaluation(evaluation);

        evaluation.UpdatedAt = DateTime.UtcNow;
        evaluation.CreatedAt = existing.CreatedAt;
        evaluation.CreatedBy = existing.CreatedBy;

        _logger.LogInformation("Mise a jour de l'evaluation {EvaluationId}.", evaluation.Id);
        return await _repository.UpdateAsync(evaluation);
    }

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant de l'evaluation est obligatoire.", nameof(id));

        await _repository.DeleteAsync(id);
        _logger.LogInformation("Evaluation {EvaluationId} supprimee.", id);
    }

    private static void ValidateEvaluation(TrainingEvaluation evaluation)
    {
        if (string.IsNullOrWhiteSpace(evaluation.SessionId))
            throw new ArgumentException("L'identifiant de la session est requis.");

        if (string.IsNullOrWhiteSpace(evaluation.EmployeeId))
            throw new ArgumentException("L'identifiant de l'employe est requis.");

        if (evaluation.Score < 0 || evaluation.Score > evaluation.MaxScore)
            throw new ArgumentException(
                $"Le score doit etre compris entre 0 et {evaluation.MaxScore}.");

        if (evaluation.MaxScore <= 0)
            throw new ArgumentException("Le score maximum doit etre superieur a 0.");
    }
}
