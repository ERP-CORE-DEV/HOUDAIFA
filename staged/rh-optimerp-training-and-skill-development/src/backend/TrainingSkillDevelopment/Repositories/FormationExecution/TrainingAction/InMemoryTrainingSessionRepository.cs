using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.Models.FormationExecution.TrainingAction;

namespace Training.SkillDevelopment.Repositories.FormationExecution.TrainingAction;

/// <summary>
/// Depot en memoire pour le developpement local sans CosmosDB.
/// NON destine a la production.
/// </summary>
public sealed class InMemoryTrainingSessionRepository : ITrainingSessionRepository
{
    private readonly ConcurrentDictionary<string, TrainingSession> _store = new();
    private readonly ILogger<InMemoryTrainingSessionRepository> _logger;

    public InMemoryTrainingSessionRepository(ILogger<InMemoryTrainingSessionRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<TrainingSession?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var session);
        return Task.FromResult(session);
    }

    public Task<IReadOnlyList<TrainingSession>> GetByActionIdAsync(string actionId)
    {
        IReadOnlyList<TrainingSession> result = _store.Values
            .Where(s => s.IsActive && s.ActionId == actionId)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<TrainingSession>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        IReadOnlyList<TrainingSession> result = _store.Values
            .Where(s => s.IsActive && s.StartDate >= start && s.StartDate <= end)
            .OrderBy(s => s.StartDate)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<TrainingSession> AddAsync(TrainingSession session)
    {
        if (string.IsNullOrWhiteSpace(session.Id))
            session.Id = Guid.NewGuid().ToString();

        session.CreatedAt = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;
        _store[session.Id] = session;
        _logger.LogInformation("Session de formation {SessionId} ajoutee en memoire.", session.Id);
        return Task.FromResult(session);
    }

    public Task<TrainingSession> UpdateAsync(TrainingSession session)
    {
        session.UpdatedAt = DateTime.UtcNow;
        _store[session.Id] = session;
        return Task.FromResult(session);
    }

    public Task DeleteAsync(string id)
    {
        if (_store.TryGetValue(id, out var session))
        {
            session.IsActive = false;
            session.UpdatedAt = DateTime.UtcNow;
            _store[id] = session;
            _logger.LogInformation("Session de formation {SessionId} supprimee (soft-delete en memoire).", id);
        }

        return Task.CompletedTask;
    }
}
