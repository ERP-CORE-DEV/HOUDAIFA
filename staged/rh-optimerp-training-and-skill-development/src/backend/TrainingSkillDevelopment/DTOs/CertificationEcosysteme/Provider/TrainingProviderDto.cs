using System.ComponentModel.DataAnnotations;
using Training.SkillDevelopment.Models.CertificationEcosysteme.Provider;

namespace Training.SkillDevelopment.DTOs.CertificationEcosysteme.Provider;

public sealed class TrainingProviderDto
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom de l'organisme de formation est obligatoire.")]
    [StringLength(200, ErrorMessage = "Le nom ne peut pas depasser 200 caracteres.")]
    public string Name { get; set; } = string.Empty;

    public string? Siret { get; set; }

    public string? DeclarationNumber { get; set; }

    public bool HasQualiopiCertification { get; set; }

    public DateTime? QualiopiExpirationDate { get; set; }

    public string? Address { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    public string? Website { get; set; }

    public string[]? Specializations { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public static TrainingProviderDto FromDomain(TrainingProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        return new TrainingProviderDto
        {
            Id = provider.Id,
            Name = provider.Name,
            Siret = provider.Siret,
            DeclarationNumber = provider.DeclarationNumber,
            HasQualiopiCertification = provider.HasQualiopiCertification,
            QualiopiExpirationDate = provider.QualiopiExpirationDate,
            Address = provider.Address,
            ContactEmail = provider.ContactEmail,
            ContactPhone = provider.ContactPhone,
            Website = provider.Website,
            Specializations = provider.Specializations,
            IsActive = provider.IsActive,
            CreatedAt = provider.CreatedAt,
            UpdatedAt = provider.UpdatedAt
        };
    }

    public TrainingProvider ToDomain()
    {
        return new TrainingProvider
        {
            Id = Id,
            Name = Name,
            Siret = Siret,
            DeclarationNumber = DeclarationNumber,
            HasQualiopiCertification = HasQualiopiCertification,
            QualiopiExpirationDate = QualiopiExpirationDate,
            Address = Address,
            ContactEmail = ContactEmail,
            ContactPhone = ContactPhone,
            Website = Website,
            Specializations = Specializations,
            IsActive = IsActive,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };
    }
}
