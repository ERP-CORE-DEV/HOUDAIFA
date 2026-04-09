using FluentValidation;
using Training.SkillDevelopment.DTOs.FormationExecution.TrainingPlan;

namespace Training.SkillDevelopment.Validators.FormationExecution.TrainingPlan;

public sealed class TrainingPlanDtoValidator : AbstractValidator<TrainingPlanDto>
{
    public TrainingPlanDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Le titre est requis")
            .MaximumLength(200).WithMessage("Le titre ne peut pas depasser 200 caracteres");

        RuleFor(x => x.Year)
            .InclusiveBetween(2020, 2050).WithMessage("L'annee doit etre entre 2020 et 2050");

        RuleFor(x => x.BudgetAllocated)
            .GreaterThanOrEqualTo(0).WithMessage("Le budget ne peut pas etre negatif");
    }
}
