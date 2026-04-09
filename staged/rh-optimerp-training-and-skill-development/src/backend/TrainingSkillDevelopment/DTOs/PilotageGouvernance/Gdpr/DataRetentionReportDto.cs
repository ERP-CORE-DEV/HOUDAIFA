namespace Training.SkillDevelopment.DTOs.PilotageGouvernance.Gdpr;

public sealed class DataRetentionReportDto
{
    public DateTime ReportDate { get; init; }

    public int TotalRecordsAnalyzed { get; init; }

    public int RecordsWithinRetention { get; init; }

    public int RecordsExpired { get; init; }

    public Dictionary<string, int> RecordsByEntity { get; init; } = new();
}
