using Newtonsoft.Json;
using Training.SkillDevelopment.Models.Auditing;
using Training.SkillDevelopment.Models.Common;

namespace Training.SkillDevelopment.Models.FormationExecution.TrainingAction;

public sealed class AttendanceRecord : IAnonymizable
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string SessionId { get; set; } = string.Empty;

    public string EmployeeId { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public AttendanceStatus Status { get; set; }

    public string? Justification { get; set; }

    public string? RecordedBy { get; set; }

    // IAnonymizable
    public bool IsAnonymized { get; set; }
    public DateTime? AnonymizationDate { get; set; }

    public void AnonymizePersonalData()
    {
        EmployeeId = "***";
        RecordedBy = "***";
        Justification = null;
        IsAnonymized = true;
        AnonymizationDate = DateTime.UtcNow;
    }
}
