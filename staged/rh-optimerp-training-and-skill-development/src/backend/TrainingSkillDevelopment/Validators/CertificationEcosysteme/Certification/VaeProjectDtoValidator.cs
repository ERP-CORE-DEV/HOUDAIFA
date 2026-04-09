using FluentValidation;
using Training.SkillDevelopment.DTOs.CertificationEcosysteme.Certification;

namespace Training.SkillDevelopment.Validators.CertificationEcosysteme.Certification;

public sealed class VaeProjectDtoValidator : AbstractValidator<VaeProjectDto>
{
    public VaeProjectDtoValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("L'identifiant du salarie est requis");

        RuleFor(x => x.CertificationId)
            .NotEmpty().WithMessage("La certification cible est requise");
    }
}
