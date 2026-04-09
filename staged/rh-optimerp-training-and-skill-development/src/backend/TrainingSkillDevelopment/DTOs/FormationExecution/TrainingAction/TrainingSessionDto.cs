using System.ComponentModel.DataAnnotations;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FormationExecution.TrainingAction;

namespace Training.SkillDevelopment.DTOs.FormationExecution.TrainingAction;

public sealed class TrainingSessionDto
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'identifiant de l'action de formation est obligatoire.")]
    public string ActionId { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string? Location { get; set; }

    public string? TrainerId { get; set; }

    public string Status { get; set; } = SessionStatus.Planned.ToString();

    public int EnrolledCount { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La capacite maximale doit etre superieure a 0.")]
    public int MaxCapacity { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public static TrainingSessionDto FromDomain(TrainingSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        return new TrainingSessionDto
        {
            Id = session.Id,
            ActionId = session.ActionId,
            StartDate = session.StartDate,
            EndDate = session.EndDate,
            Location = session.Location,
            TrainerId = session.TrainerId,
            Status = session.Status.ToString(),
            EnrolledCount = session.EnrolledCount,
            MaxCapacity = session.MaxCapacity,
            Notes = session.Notes,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt,
            IsActive = session.IsActive
        };
    }

    public TrainingSession ToDomain()
    {
        if (!Enum.TryParse<SessionStatus>(Status, out var status))
            throw new ArgumentException($"Statut de session invalide : '{Status}'.");

        return new TrainingSession
        {
            Id = Id,
            ActionId = ActionId,
            StartDate = StartDate,
            EndDate = EndDate,
            Location = Location,
            TrainerId = TrainerId,
            Status = status,
            EnrolledCount = EnrolledCount,
            MaxCapacity = MaxCapacity,
            Notes = Notes,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt,
            IsActive = IsActive
        };
    }
}
