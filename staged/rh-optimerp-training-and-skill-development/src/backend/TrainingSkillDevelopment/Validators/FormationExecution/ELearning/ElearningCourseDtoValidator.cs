using FluentValidation;
using Training.SkillDevelopment.DTOs.FormationExecution.ELearning;

namespace Training.SkillDevelopment.Validators.FormationExecution.ELearning;

public sealed class ElearningCourseDtoValidator : AbstractValidator<ElearningCourseDto>
{
    public ElearningCourseDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Le titre du cours est requis");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("La duree doit etre positive");
    }
}
