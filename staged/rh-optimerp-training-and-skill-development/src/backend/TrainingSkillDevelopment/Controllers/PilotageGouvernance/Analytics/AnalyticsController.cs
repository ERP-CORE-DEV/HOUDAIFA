using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.SkillDevelopment.DTOs.PilotageGouvernance.Analytics;
using Training.SkillDevelopment.Services.PilotageGouvernance.Analytics;

namespace Training.SkillDevelopment.Controllers.PilotageGouvernance.Analytics;

[Authorize]
[ApiController]
[Route("api/training-analytics")]
[Produces("application/json")]
public sealed class AnalyticsController : ControllerBase
{
    private readonly ITrainingAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        ITrainingAnalyticsService analyticsService,
        ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("dashboard-kpis")]
    [ProducesResponseType(typeof(TrainingDashboardKpiDto), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<TrainingDashboardKpiDto>> GetDashboardKpisAsync([FromQuery] int? year)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        _logger.LogInformation("Recuperation des KPIs du tableau de bord pour l'annee {Year}.", targetYear);

        var kpis = await _analyticsService.GetDashboardKpisAsync(targetYear);
        return Ok(kpis);
    }

    [HttpGet("bilan-social")]
    [ProducesResponseType(typeof(BilanSocialTrainingDto), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<BilanSocialTrainingDto>> GetBilanSocialAsync([FromQuery] int? year)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        _logger.LogInformation("Recuperation du bilan social formation pour l'annee {Year}.", targetYear);

        var bilan = await _analyticsService.GetBilanSocialTrainingAsync(targetYear);
        return Ok(bilan);
    }

    [HttpGet("gender-equality-report")]
    [ProducesResponseType(typeof(GenderEqualityTrainingReportDto), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<GenderEqualityTrainingReportDto>> GetGenderEqualityReportAsync([FromQuery] int? year)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        _logger.LogInformation("Recuperation du rapport egalite femmes-hommes pour l'annee {Year}.", targetYear);

        var report = await _analyticsService.GetGenderEqualityReportAsync(targetYear);
        return Ok(report);
    }

    [HttpGet("roi/{trainingActionId}")]
    [ProducesResponseType(typeof(decimal), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<decimal>> CalculateRoiAsync(string trainingActionId)
    {
        _logger.LogInformation("Calcul du ROI pour l'action de formation {ActionId}.", trainingActionId);

        var roi = await _analyticsService.CalculateTrainingRoiAsync(trainingActionId);
        return Ok(roi);
    }

    [HttpGet("trends")]
    [ProducesResponseType(typeof(IReadOnlyList<TrainingTrendDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<IReadOnlyList<TrainingTrendDto>>> GetTrendsAsync(
        [FromQuery] int? startYear,
        [FromQuery] int? endYear)
    {
        var currentYear = DateTime.UtcNow.Year;
        var from = startYear ?? currentYear - 3;
        var to = endYear ?? currentYear;

        _logger.LogInformation("Recuperation des tendances de formation de {StartYear} a {EndYear}.", from, to);

        var trends = await _analyticsService.GetTrainingTrendsAsync(from, to);
        return Ok(trends);
    }
}
