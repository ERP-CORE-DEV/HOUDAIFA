using System.ComponentModel.DataAnnotations;
using Training.SkillDevelopment.Models.CertificationEcosysteme.Certification;

namespace Training.SkillDevelopment.DTOs.CertificationEcosysteme.Certification;

public sealed class CertificationRncpDto
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le code RNCP est obligatoire.")]
    [RegularExpression(@"^(RNCP|RS)\d+$", ErrorMessage = "Le code RNCP doit etre au format RNCPXXXXX ou RSXXXXX.")]
    public string RncpCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le titre de la certification est obligatoire.")]
    [StringLength(300, ErrorMessage = "Le titre ne peut pas depasser 300 caracteres.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? CertifyingBody { get; set; }

    public int? NsfCode { get; set; }

    public string? Level { get; set; }

    public DateTime? RegistrationDate { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public string[]? EligibleOpcoIds { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public static CertificationRncpDto FromDomain(CertificationRncp certification)
    {
        ArgumentNullException.ThrowIfNull(certification);

        return new CertificationRncpDto
        {
            Id = certification.Id,
            RncpCode = certification.RncpCode,
            Title = certification.Title,
            Description = certification.Description,
            CertifyingBody = certification.CertifyingBody,
            NsfCode = certification.NsfCode,
            Level = certification.Level,
            RegistrationDate = certification.RegistrationDate,
            ExpirationDate = certification.ExpirationDate,
            EligibleOpcoIds = certification.EligibleOpcoIds,
            IsActive = certification.IsActive,
            CreatedAt = certification.CreatedAt,
            UpdatedAt = certification.UpdatedAt
        };
    }

    public CertificationRncp ToDomain()
    {
        return new CertificationRncp
        {
            Id = Id,
            RncpCode = RncpCode,
            Title = Title,
            Description = Description,
            CertifyingBody = CertifyingBody,
            NsfCode = NsfCode,
            Level = Level,
            RegistrationDate = RegistrationDate,
            ExpirationDate = ExpirationDate,
            EligibleOpcoIds = EligibleOpcoIds,
            IsActive = IsActive,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };
    }
}
