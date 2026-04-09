using Training.SkillDevelopment.Models.FinancementConformite.Opco;

namespace Training.SkillDevelopment.DTOs.FinancementConformite.Opco;

public sealed class OpcoDto
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Website { get; set; }

    public string? ContactInfo { get; set; }

    public string[]? ConventionCollectiveIds { get; set; }

    public bool IsActive { get; set; }

    public static OpcoDto FromDomain(Models.FinancementConformite.Opco.Opco opco)
    {
        ArgumentNullException.ThrowIfNull(opco);

        return new OpcoDto
        {
            Id = opco.Id,
            Name = opco.Name,
            Code = opco.Code,
            Website = opco.Website,
            ContactInfo = opco.ContactInfo,
            ConventionCollectiveIds = opco.ConventionCollectiveIds,
            IsActive = opco.IsActive
        };
    }

    public Models.FinancementConformite.Opco.Opco ToDomain()
    {
        return new Models.FinancementConformite.Opco.Opco
        {
            Id = Id,
            Name = Name,
            Code = Code,
            Website = Website,
            ContactInfo = ContactInfo,
            ConventionCollectiveIds = ConventionCollectiveIds,
            IsActive = IsActive
        };
    }
}
