using FluentValidation;
using Training.SkillDevelopment.DTOs.FinancementConformite.Cpf;

namespace Training.SkillDevelopment.Validators.FinancementConformite.Cpf;

public sealed class CpfAccountDtoValidator : AbstractValidator<CpfAccountDto>
{
    public CpfAccountDtoValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("L'identifiant du salarie est requis");

        RuleFor(x => x.BalanceEuros)
            .GreaterThanOrEqualTo(0).WithMessage("Le solde ne peut pas etre negatif");

        RuleFor(x => x.CeilingEuros)
            .InclusiveBetween(5000, 8000).WithMessage("Le plafond doit etre entre 5000 et 8000 EUR");
    }
}
