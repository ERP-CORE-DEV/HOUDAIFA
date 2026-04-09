using FluentValidation;
using Training.SkillDevelopment.DTOs.FinancementConformite.Opco;

namespace Training.SkillDevelopment.Validators.FinancementConformite.Opco;

public sealed class FundingApplicationDtoValidator : AbstractValidator<FundingApplicationDto>
{
    public FundingApplicationDtoValidator()
    {
        RuleFor(x => x.OpcoId)
            .NotEmpty().WithMessage("L'OPCO est requis");

        RuleFor(x => x.TrainingActionId)
            .NotEmpty().WithMessage("L'action de formation est requise");

        RuleFor(x => x.RequestedAmount)
            .GreaterThan(0).WithMessage("Le montant demande doit etre superieur a 0");
    }
}
