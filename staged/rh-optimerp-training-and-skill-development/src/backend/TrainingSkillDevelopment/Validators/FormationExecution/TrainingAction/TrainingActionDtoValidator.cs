using FluentValidation;
using Training.SkillDevelopment.DTOs.FormationExecution.TrainingAction;

namespace Training.SkillDevelopment.Validators.FormationExecution.TrainingAction;

public sealed class TrainingActionDtoValidator : AbstractValidator<TrainingActionDto>
{
    public TrainingActionDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Le titre de l'action est requis")
            .MaximumLength(300).WithMessage("Le titre ne peut pas depasser 300 caracteres");

        RuleFor(x => x.DurationHours)
            .GreaterThan(0).WithMessage("La duree doit etre superieure a 0");

        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0).WithMessage("Le cout ne peut pas etre negatif");

        RuleFor(x => x.MaxParticipants)
            .GreaterThan(0).When(x => x.MaxParticipants > 0)
            .WithMessage("Le nombre maximum de participants doit etre positif");
    }
}
