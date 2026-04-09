using FluentValidation;
using Training.SkillDevelopment.DTOs.FormationExecution.TrainingAction;

namespace Training.SkillDevelopment.Validators.FormationExecution.TrainingAction;

public sealed class TrainingSessionDtoValidator : AbstractValidator<TrainingSessionDto>
{
    public TrainingSessionDtoValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("La date de debut est requise");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("La date de fin doit etre apres la date de debut");

        RuleFor(x => x.MaxCapacity)
            .GreaterThan(0).WithMessage("La capacite maximale doit etre positive");
    }
}
