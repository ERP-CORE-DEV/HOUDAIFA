using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.Models.FormationExecution.Evaluation;

namespace Training.SkillDevelopment.Repositories.FormationExecution.Evaluation;

/// <summary>
/// Depot en memoire pour le developpement local sans CosmosDB.
/// NON destine a la production.
/// </summary>
public sealed class InMemoryTrainingEvaluationRepository : ITrainingEvaluationRepository
{
    private readonly ConcurrentDictionary<string, TrainingEvaluation> _store = new();
    private readonly ILogger<InMemoryTrainingEvaluationRepository> _logger;

    public InMemoryTrainingEvaluationRepository(ILogger<InMemoryTrainingEvaluationRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<TrainingEvaluation?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var evaluation);
        return Task.FromResult(evaluation);
    }

    public Task<IReadOnlyList<TrainingEvaluation>> GetBySessionIdAsync(string sessionId)
    {
        IReadOnlyList<TrainingEvaluation> result = _store.Values
            .Where(e => e.SessionId == sessionId)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<TrainingEvaluation>> GetByEmployeeIdAsync(string employeeId)
    {
        IReadOnlyList<TrainingEvaluation> result = _store.Values
            .Where(e => e.EmployeeId == employeeId)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<decimal> GetAverageScoreAsync(string sessionId)
    {
        var evaluations = _store.Values
            .Where(e => e.SessionId == sessionId)
            .ToList();

        if (evaluations.Count == 0)
            return Task.FromResult(0m);

        var average = evaluations.Average(e => e.Score);
        return Task.FromResult(Math.Round(average, 2));
    }

    public Task<TrainingEvaluation> AddAsync(TrainingEvaluation evaluation)
    {
        if (string.IsNullOrWhiteSpace(evaluation.Id))
            evaluation.Id = Guid.NewGuid().ToString();

        evaluation.CreatedAt = DateTime.UtcNow;
        evaluation.UpdatedAt = DateTime.UtcNow;
        _store[evaluation.Id] = evaluation;
        _logger.LogInformation("Evaluation de formation {EvaluationId} ajoutee en memoire.", evaluation.Id);
        return Task.FromResult(evaluation);
    }

    public Task<TrainingEvaluation> UpdateAsync(TrainingEvaluation evaluation)
    {
        evaluation.UpdatedAt = DateTime.UtcNow;
        _store[evaluation.Id] = evaluation;
        return Task.FromResult(evaluation);
    }

    public Task DeleteAsync(string id)
    {
        _store.TryRemove(id, out _);
        _logger.LogInformation("Evaluation de formation {EvaluationId} supprimee en memoire.", id);
        return Task.CompletedTask;
    }
}
