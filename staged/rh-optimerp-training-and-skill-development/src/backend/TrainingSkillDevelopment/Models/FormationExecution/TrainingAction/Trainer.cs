using Newtonsoft.Json;
using Training.SkillDevelopment.Models.Auditing;

namespace Training.SkillDevelopment.Models.FormationExecution.TrainingAction;

public sealed class Trainer : IAnonymizable
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string Type { get; set; } = "Internal";

    public string[]? Specializations { get; set; }

    public decimal? HourlyRate { get; set; }

    public bool IsInternal { get; set; }

    public string[]? Qualifications { get; set; }

    public bool IsActive { get; set; } = true;

    public string FullName => $"{FirstName} {LastName}".Trim();

    // IAnonymizable
    public bool IsAnonymized { get; set; }
    public DateTime? AnonymizationDate { get; set; }

    public void AnonymizePersonalData()
    {
        FirstName = "***";
        LastName = "***";
        Email = "***@***.***";
        IsAnonymized = true;
        AnonymizationDate = DateTime.UtcNow;
    }
}
