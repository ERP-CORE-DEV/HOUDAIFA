using FluentValidation;
using Training.SkillDevelopment.DTOs.CertificationEcosysteme.Certification;

namespace Training.SkillDevelopment.Validators.CertificationEcosysteme.Certification;

public sealed class CertificationRncpDtoValidator : AbstractValidator<CertificationRncpDto>
{
    public CertificationRncpDtoValidator()
    {
        RuleFor(x => x.RncpCode)
            .NotEmpty().WithMessage("Le code RNCP/RS est requis")
            .Matches(@"^(RNCP|RS)\d+$").WithMessage("Le code RNCP/RS est invalide (format: RNCP12345 ou RS12345)");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Le titre de la certification est requis");

        RuleFor(x => x.CertifyingBody)
            .NotEmpty().WithMessage("L'organisme certificateur est requis");
    }
}
