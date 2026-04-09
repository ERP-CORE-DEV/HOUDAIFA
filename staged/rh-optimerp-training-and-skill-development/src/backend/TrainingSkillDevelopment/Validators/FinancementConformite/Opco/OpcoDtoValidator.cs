using FluentValidation;
using Training.SkillDevelopment.DTOs.FinancementConformite.Opco;

namespace Training.SkillDevelopment.Validators.FinancementConformite.Opco;

public sealed class OpcoDtoValidator : AbstractValidator<OpcoDto>
{
    public OpcoDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Le nom de l'OPCO est requis");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Le code OPCO est requis");
    }
}
