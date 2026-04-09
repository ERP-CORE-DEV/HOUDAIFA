using System.ComponentModel.DataAnnotations;
using Training.SkillDevelopment.Models.CertificationEcosysteme.Certification;
using Training.SkillDevelopment.Models.Common;

namespace Training.SkillDevelopment.DTOs.CertificationEcosysteme.Certification;

public sealed class VaeProjectDto
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'identifiant de l'employe est obligatoire.")]
    public string EmployeeId { get; set; } = string.Empty;

    public string? CertificationId { get; set; }

    public string? RncpCode { get; set; }

    public string Status { get; set; } = VaeStatus.Recevabilite.ToString();

    public DateTime StartDate { get; set; }

    public DateTime? ExpectedJuryDate { get; set; }

    public DateTime? JuryDate { get; set; }

    public string? AccompanimentOrganization { get; set; }

    public string? Notes { get; set; }

    public bool IsAnonymized { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public static VaeProjectDto FromDomain(VaeProject project)
    {
        ArgumentNullException.ThrowIfNull(project);

        return new VaeProjectDto
        {
            Id = project.Id,
            EmployeeId = project.EmployeeId,
            CertificationId = project.CertificationId,
            RncpCode = project.RncpCode,
            Status = project.Status.ToString(),
            StartDate = project.StartDate,
            ExpectedJuryDate = project.ExpectedJuryDate,
            JuryDate = project.JuryDate,
            AccompanimentOrganization = project.AccompanimentOrganization,
            Notes = project.Notes,
            IsAnonymized = project.IsAnonymized,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };
    }

    public VaeProject ToDomain()
    {
        if (!Enum.TryParse<VaeStatus>(Status, out var status))
            throw new ArgumentException($"Statut VAE invalide : '{Status}'.");

        return new VaeProject
        {
            Id = Id,
            EmployeeId = EmployeeId,
            CertificationId = CertificationId,
            RncpCode = RncpCode,
            Status = status,
            StartDate = StartDate,
            ExpectedJuryDate = ExpectedJuryDate,
            JuryDate = JuryDate,
            AccompanimentOrganization = AccompanimentOrganization,
            Notes = Notes,
            IsAnonymized = IsAnonymized,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };
    }
}
