using System.ComponentModel.DataAnnotations;
using Training.SkillDevelopment.Models.FormationExecution.ELearning;

namespace Training.SkillDevelopment.DTOs.FormationExecution.ELearning;

public sealed class LearnerProgressDto
{
    public string Id { get; init; } = string.Empty;

    [Required(ErrorMessage = "L'identifiant de l'employe est obligatoire.")]
    public string EmployeeId { get; init; } = string.Empty;

    [Required(ErrorMessage = "L'identifiant du cours est obligatoire.")]
    public string CourseId { get; init; } = string.Empty;

    public string Status { get; init; } = "NotStarted";

    [Range(0, 100, ErrorMessage = "Le pourcentage de progression doit etre compris entre 0 et 100.")]
    public int ProgressPercentage { get; init; }

    public int TimeSpentMinutes { get; init; }

    [Range(0, 100, ErrorMessage = "Le score doit etre compris entre 0 et 100.")]
    public int? Score { get; init; }

    public DateTime? StartedAt { get; init; }

    public DateTime? CompletedAt { get; init; }

    public bool IsAnonymized { get; init; }

    public static LearnerProgressDto FromDomain(LearnerProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);

        return new LearnerProgressDto
        {
            Id = progress.Id,
            EmployeeId = progress.EmployeeId,
            CourseId = progress.CourseId,
            Status = progress.Status,
            ProgressPercentage = progress.ProgressPercentage,
            TimeSpentMinutes = progress.TimeSpentMinutes,
            Score = progress.Score,
            StartedAt = progress.StartedAt,
            CompletedAt = progress.CompletedAt,
            IsAnonymized = progress.IsAnonymized
        };
    }

    public LearnerProgress ToDomain()
    {
        return new LearnerProgress
        {
            Id = Id,
            EmployeeId = EmployeeId,
            CourseId = CourseId,
            Status = Status,
            ProgressPercentage = ProgressPercentage,
            TimeSpentMinutes = TimeSpentMinutes,
            Score = Score,
            StartedAt = StartedAt,
            CompletedAt = CompletedAt,
            IsAnonymized = IsAnonymized
        };
    }
}
