using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.Models.FormationExecution.TrainingAction;

namespace Training.SkillDevelopment.Repositories.FormationExecution.TrainingAction;

/// <summary>
/// Depot en memoire pour le developpement local sans CosmosDB.
/// NON destine a la production.
/// </summary>
public sealed class InMemoryEnrollmentRepository : IEnrollmentRepository
{
    private readonly ConcurrentDictionary<string, Enrollment> _store = new();
    private readonly ILogger<InMemoryEnrollmentRepository> _logger;

    public InMemoryEnrollmentRepository(ILogger<InMemoryEnrollmentRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Enrollment?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var enrollment);
        return Task.FromResult(enrollment);
    }

    public Task<IReadOnlyList<Enrollment>> GetBySessionIdAsync(string sessionId)
    {
        IReadOnlyList<Enrollment> result = _store.Values
            .Where(e => e.SessionId == sessionId)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<Enrollment>> GetByEmployeeIdAsync(string employeeId)
    {
        IReadOnlyList<Enrollment> result = _store.Values
            .Where(e => e.EmployeeId == employeeId)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<Enrollment> AddAsync(Enrollment enrollment)
    {
        if (string.IsNullOrWhiteSpace(enrollment.Id))
            enrollment.Id = Guid.NewGuid().ToString();

        enrollment.EnrolledAt = DateTime.UtcNow;
        _store[enrollment.Id] = enrollment;
        _logger.LogInformation("Inscription {EnrollmentId} ajoutee en memoire.", enrollment.Id);
        return Task.FromResult(enrollment);
    }

    public Task<Enrollment> UpdateAsync(Enrollment enrollment)
    {
        _store[enrollment.Id] = enrollment;
        return Task.FromResult(enrollment);
    }

    public Task DeleteAsync(string id)
    {
        _store.TryRemove(id, out _);
        _logger.LogInformation("Inscription {EnrollmentId} supprimee en memoire.", id);
        return Task.CompletedTask;
    }
}
