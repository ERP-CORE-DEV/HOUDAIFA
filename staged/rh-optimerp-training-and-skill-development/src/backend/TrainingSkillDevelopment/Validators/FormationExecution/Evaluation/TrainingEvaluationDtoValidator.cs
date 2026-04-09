using FluentValidation;
using Training.SkillDevelopment.DTOs.FormationExecution.Evaluation;

namespace Training.SkillDevelopment.Validators.FormationExecution.Evaluation;

public sealed class TrainingEvaluationDtoValidator : AbstractValidator<TrainingEvaluationDto>
{
    public TrainingEvaluationDtoValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("L'identifiant de la session est requis");

        RuleFor(x => x.Score)
            .InclusiveBetween(1, 5).WithMessage("La note doit etre entre 1 et 5");
    }
}
