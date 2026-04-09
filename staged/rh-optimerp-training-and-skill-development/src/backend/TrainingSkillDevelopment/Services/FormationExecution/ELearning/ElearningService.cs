using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FormationExecution.ELearning;
using Training.SkillDevelopment.Repositories.FormationExecution.ELearning;

namespace Training.SkillDevelopment.Services.FormationExecution.ELearning;

public sealed class ElearningService : IElearningService
{
    private const int MaxProgressPercentage = 100;

    private readonly IElearningCourseRepository _repository;
    private readonly ILogger<ElearningService> _logger;

    public ElearningService(
        IElearningCourseRepository repository,
        ILogger<ElearningService> logger)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(logger);
        _repository = repository;
        _logger = logger;
    }

    public async Task<ElearningCourse?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant du cours est obligatoire.", nameof(id));

        return await _repository.GetByIdAsync(id);
    }

    public async Task<PagedResult<ElearningCourse>> GetPagedAsync(int page, int pageSize)
    {
        if (page < 1)
            throw new ArgumentException("Le numero de page doit etre superieur a 0.", nameof(page));

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException("La taille de la page doit etre comprise entre 1 et 100.", nameof(pageSize));

        return await _repository.GetPagedAsync(page, pageSize);
    }

    public async Task<IReadOnlyList<ElearningCourse>> GetByTagAsync(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentException("Le tag est obligatoire.", nameof(tag));

        return await _repository.GetByTagAsync(tag);
    }

    public async Task<ElearningCourse> CreateAsync(ElearningCourse course)
    {
        ArgumentNullException.ThrowIfNull(course);
        ValidateCourse(course);

        course.Id = Guid.NewGuid().ToString();
        course.IsActive = true;
        course.CreatedAt = DateTime.UtcNow;
        course.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation("Creation du cours e-learning '{Title}'.", course.Title);
        return await _repository.AddAsync(course);
    }

    public async Task<ElearningCourse> UpdateAsync(ElearningCourse course)
    {
        ArgumentNullException.ThrowIfNull(course);

        var existing = await _repository.GetByIdAsync(course.Id)
            ?? throw new KeyNotFoundException($"Le cours '{course.Id}' est introuvable.");

        ValidateCourse(course);

        course.UpdatedAt = DateTime.UtcNow;
        course.CreatedAt = existing.CreatedAt;

        _logger.LogInformation("Mise a jour du cours e-learning {CourseId}.", course.Id);
        return await _repository.UpdateAsync(course);
    }

    public async Task<LearnerProgress> TrackProgressAsync(LearnerProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);

        if (string.IsNullOrWhiteSpace(progress.EmployeeId))
            throw new ArgumentException("L'identifiant de l'employe est requis.");

        if (string.IsNullOrWhiteSpace(progress.CourseId))
            throw new ArgumentException("L'identifiant du cours est requis.");

        if (progress.ProgressPercentage < 0 || progress.ProgressPercentage > MaxProgressPercentage)
            throw new ArgumentException("Le pourcentage de progression doit etre compris entre 0 et 100.");

        if (progress.ProgressPercentage == MaxProgressPercentage && progress.Status != "Completed")
        {
            progress.Status = "Completed";
            progress.CompletedAt = DateTime.UtcNow;
        }
        else if (progress.ProgressPercentage > 0 && progress.Status == "NotStarted")
        {
            progress.Status = "InProgress";
            progress.StartedAt ??= DateTime.UtcNow;
        }

        _logger.LogInformation(
            "Progression du cours {CourseId} pour l'employe {EmployeeId}: {Progress}%.",
            progress.CourseId, progress.EmployeeId, progress.ProgressPercentage);

        return await Task.FromResult(progress);
    }

    public async Task<decimal> GetCompletionRateAsync(string courseId)
    {
        if (string.IsNullOrWhiteSpace(courseId))
            throw new ArgumentException("L'identifiant du cours est obligatoire.", nameof(courseId));

        var course = await _repository.GetByIdAsync(courseId)
            ?? throw new KeyNotFoundException($"Le cours '{courseId}' est introuvable.");

        _logger.LogInformation("Calcul du taux de completion pour le cours {CourseId}.", courseId);

        return 0m;
    }

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant du cours est obligatoire.", nameof(id));

        await _repository.DeleteAsync(id);
        _logger.LogInformation("Cours e-learning {CourseId} supprime.", id);
    }

    private static void ValidateCourse(ElearningCourse course)
    {
        if (string.IsNullOrWhiteSpace(course.Title))
            throw new ArgumentException("Le titre du cours est requis.");

        if (string.IsNullOrWhiteSpace(course.Format))
            throw new ArgumentException("Le format du cours est requis.");

        if (course.DurationMinutes <= 0)
            throw new ArgumentException("La duree du cours en minutes doit etre superieure a 0.");
    }
}
