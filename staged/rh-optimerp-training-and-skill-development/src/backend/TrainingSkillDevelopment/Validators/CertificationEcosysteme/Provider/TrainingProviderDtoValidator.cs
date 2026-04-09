using FluentValidation;
using Training.SkillDevelopment.DTOs.CertificationEcosysteme.Provider;

namespace Training.SkillDevelopment.Validators.CertificationEcosysteme.Provider;

public sealed class TrainingProviderDtoValidator : AbstractValidator<TrainingProviderDto>
{
    public TrainingProviderDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Le nom de l'organisme est requis");

        RuleFor(x => x.DeclarationNumber)
            .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.DeclarationNumber))
            .WithMessage("Le numero de declaration ne peut pas depasser 20 caracteres");
    }
}
