using System.ComponentModel.DataAnnotations;
using Training.SkillDevelopment.Models.FormationExecution.ELearning;

namespace Training.SkillDevelopment.DTOs.FormationExecution.ELearning;

public sealed class ElearningCourseDto
{
    public string Id { get; init; } = string.Empty;

    [Required(ErrorMessage = "Le titre du cours e-learning est obligatoire.")]
    [StringLength(200, ErrorMessage = "Le titre ne peut pas depasser 200 caracteres.")]
    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    [Required(ErrorMessage = "Le format du cours est obligatoire.")]
    public string Format { get; init; } = string.Empty;

    public string? ScormPackageId { get; init; }

    [Range(1, 10000, ErrorMessage = "La duree doit etre comprise entre 1 et 10 000 minutes.")]
    public int DurationMinutes { get; init; }

    public string? Provider { get; init; }

    public string[]? Tags { get; init; }

    public string[]? CompetencyIds { get; init; }

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }

    public static ElearningCourseDto FromDomain(ElearningCourse course)
    {
        ArgumentNullException.ThrowIfNull(course);

        return new ElearningCourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Format = course.Format,
            ScormPackageId = course.ScormPackageId,
            DurationMinutes = course.DurationMinutes,
            Provider = course.Provider,
            Tags = course.Tags,
            CompetencyIds = course.CompetencyIds,
            IsActive = course.IsActive,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt
        };
    }

    public ElearningCourse ToDomain()
    {
        return new ElearningCourse
        {
            Id = Id,
            Title = Title,
            Description = Description,
            Format = Format,
            ScormPackageId = ScormPackageId,
            DurationMinutes = DurationMinutes,
            Provider = Provider,
            Tags = Tags,
            CompetencyIds = CompetencyIds,
            IsActive = IsActive,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };
    }
}
