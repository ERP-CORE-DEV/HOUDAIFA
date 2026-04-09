using Newtonsoft.Json;
using Training.SkillDevelopment.Models.Auditing;
using Training.SkillDevelopment.Models.Common;

namespace Training.SkillDevelopment.Models.FormationExecution.TrainingAction;

public sealed class Enrollment : IAnonymizable
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    public string SessionId { get; set; } = string.Empty;

    public string EmployeeId { get; set; } = string.Empty;

    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Requested;

    public DateTime EnrolledAt { get; set; }

    public string? ValidatedBy { get; set; }

    public DateTime? ConvocationSentAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string? CancellationReason { get; set; }

    // IAnonymizable
    public bool IsAnonymized { get; set; }
    public DateTime? AnonymizationDate { get; set; }

    public void AnonymizePersonalData()
    {
        EmployeeId = "***";
        ValidatedBy = "***";
        IsAnonymized = true;
        AnonymizationDate = DateTime.UtcNow;
    }
}
