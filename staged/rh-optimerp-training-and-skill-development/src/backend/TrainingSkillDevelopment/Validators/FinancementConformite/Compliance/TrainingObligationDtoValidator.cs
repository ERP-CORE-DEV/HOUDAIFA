using FluentValidation;
using Training.SkillDevelopment.DTOs.FinancementConformite.Compliance;

namespace Training.SkillDevelopment.Validators.FinancementConformite.Compliance;

public sealed class TrainingObligationDtoValidator : AbstractValidator<TrainingObligationDto>
{
    public TrainingObligationDtoValidator()
    {
        RuleFor(x => x.RegulatoryReference)
            .NotEmpty().WithMessage("La reference reglementaire est requise");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La description est requise");

        RuleFor(x => x.FrequencyMonths)
            .GreaterThan(0).WithMessage("La periode de renouvellement doit etre positive");
    }
}
