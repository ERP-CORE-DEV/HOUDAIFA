using System.ComponentModel.DataAnnotations;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FormationExecution.Evaluation;

namespace Training.SkillDevelopment.DTOs.FormationExecution.Evaluation;

public sealed class TrainingEvaluationDto
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'identifiant de la session est obligatoire.")]
    public string SessionId { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'identifiant de l'employe est obligatoire.")]
    public string EmployeeId { get; set; } = string.Empty;

    public EvaluationLevel Level { get; set; }

    [Range(0, 10, ErrorMessage = "Le score doit etre compris entre 0 et 10.")]
    public decimal Score { get; set; }

    public int MaxScore { get; set; } = 10;

    public string? Comments { get; set; }

    public DateTime EvaluatedAt { get; set; }

    public string? EvaluatorId { get; set; }

    public bool IsAnonymized { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public static TrainingEvaluationDto FromDomain(TrainingEvaluation evaluation)
    {
        ArgumentNullException.ThrowIfNull(evaluation);

        return new TrainingEvaluationDto
        {
            Id = evaluation.Id,
            SessionId = evaluation.SessionId,
            EmployeeId = evaluation.EmployeeId,
            Level = evaluation.Level,
            Score = evaluation.Score,
            MaxScore = evaluation.MaxScore,
            Comments = evaluation.Comments,
            EvaluatedAt = evaluation.EvaluatedAt,
            EvaluatorId = evaluation.EvaluatorId,
            IsAnonymized = evaluation.IsAnonymized,
            CreatedAt = evaluation.CreatedAt,
            UpdatedAt = evaluation.UpdatedAt
        };
    }

    public TrainingEvaluation ToDomain()
    {
        return new TrainingEvaluation
        {
            Id = Id,
            SessionId = SessionId,
            EmployeeId = EmployeeId,
            Level = Level,
            Score = Score,
            MaxScore = MaxScore,
            Comments = Comments,
            EvaluatedAt = EvaluatedAt,
            EvaluatorId = EvaluatorId,
            IsAnonymized = IsAnonymized,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };
    }
}
