using Newtonsoft.Json;
using Training.SkillDevelopment.Models.Auditing;

namespace Training.SkillDevelopment.Models.CertificationEcosysteme.Provider;

public sealed class TrainingProvider : IAuditable, ISoftDeletable
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Siret { get; set; }

    /// <summary>
    /// Numero de declaration d'activite (NDA) - obligatoire pour les organismes de formation.
    /// </summary>
    public string? DeclarationNumber { get; set; }

    /// <summary>
    /// Certification Qualiopi pour les prestataires de formation (L6351-1 du Code du travail).
    /// </summary>
    public bool HasQualiopiCertification { get; set; }

    public DateTime? QualiopiExpirationDate { get; set; }

    public string? Address { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    public string? Website { get; set; }

    public string[]? Specializations { get; set; }

    // IAuditable
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // ISoftDeletable
    public bool IsActive { get; set; } = true;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
