using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FormationExecution.ELearning;

namespace Training.SkillDevelopment.Repositories.FormationExecution.ELearning;

/// <summary>
/// Depot en memoire pour le developpement local sans CosmosDB.
/// NON destine a la production.
/// </summary>
public sealed class InMemoryElearningCourseRepository : IElearningCourseRepository
{
    private readonly ConcurrentDictionary<string, ElearningCourse> _store = new();
    private readonly ILogger<InMemoryElearningCourseRepository> _logger;

    public InMemoryElearningCourseRepository(ILogger<InMemoryElearningCourseRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<ElearningCourse?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var course);
        return Task.FromResult(course);
    }

    public Task<IReadOnlyList<ElearningCourse>> GetAllAsync()
    {
        IReadOnlyList<ElearningCourse> result = _store.Values
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.CreatedAt)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<ElearningCourse>> GetByTagAsync(string tag)
    {
        IReadOnlyList<ElearningCourse> result = _store.Values
            .Where(c => c.IsActive && c.Tags != null && c.Tags.Contains(tag))
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<ElearningCourse>> GetByCompetencyIdAsync(string competencyId)
    {
        IReadOnlyList<ElearningCourse> result = _store.Values
            .Where(c => c.IsActive && c.CompetencyIds != null && c.CompetencyIds.Contains(competencyId))
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<ElearningCourse> AddAsync(ElearningCourse course)
    {
        if (string.IsNullOrWhiteSpace(course.Id))
            course.Id = Guid.NewGuid().ToString();

        course.CreatedAt = DateTime.UtcNow;
        course.UpdatedAt = DateTime.UtcNow;
        _store[course.Id] = course;
        _logger.LogInformation("Cours e-learning {CourseId} ajoute en memoire.", course.Id);
        return Task.FromResult(course);
    }

    public Task<ElearningCourse> UpdateAsync(ElearningCourse course)
    {
        course.UpdatedAt = DateTime.UtcNow;
        _store[course.Id] = course;
        return Task.FromResult(course);
    }

    public Task<bool> DeleteAsync(string id)
    {
        if (_store.TryGetValue(id, out var course))
        {
            course.IsActive = false;
            _store[id] = course;
            _logger.LogInformation("Cours e-learning {CourseId} supprime en memoire.", id);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public Task<PagedResult<ElearningCourse>> GetPagedAsync(int page, int pageSize)
    {
        var active = _store.Values
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

        var items = active.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult(new PagedResult<ElearningCourse>
        {
            Items = items.AsReadOnly(),
            TotalCount = active.Count,
            Page = page,
            PageSize = pageSize
        });
    }
}
