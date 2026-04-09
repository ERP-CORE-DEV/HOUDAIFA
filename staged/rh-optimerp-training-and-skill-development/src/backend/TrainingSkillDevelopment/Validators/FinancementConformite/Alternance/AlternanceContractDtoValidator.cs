using FluentValidation;
using Training.SkillDevelopment.DTOs.FinancementConformite.Alternance;

namespace Training.SkillDevelopment.Validators.FinancementConformite.Alternance;

public sealed class AlternanceContractDtoValidator : AbstractValidator<AlternanceContractDto>
{
    public AlternanceContractDtoValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("L'identifiant du salarie est requis");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("La date de debut est requise");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("La date de fin doit etre apres la date de debut");

        RuleFor(x => x.RemunerationPercentage)
            .InclusiveBetween(0, 100).WithMessage("Le pourcentage de remuneration doit etre entre 0 et 100");
    }
}
