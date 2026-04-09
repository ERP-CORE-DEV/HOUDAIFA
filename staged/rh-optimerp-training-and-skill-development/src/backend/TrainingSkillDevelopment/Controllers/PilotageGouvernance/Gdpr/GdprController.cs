using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.SkillDevelopment.DTOs.PilotageGouvernance.Gdpr;
using Training.SkillDevelopment.Services.PilotageGouvernance.Gdpr;

namespace Training.SkillDevelopment.Controllers.PilotageGouvernance.Gdpr;

[Authorize]
[ApiController]
[Route("api/gdpr")]
[Produces("application/json")]
public sealed class GdprController : ControllerBase
{
    private readonly IGdprService _gdprService;
    private readonly ILogger<GdprController> _logger;

    public GdprController(
        IGdprService gdprService,
        ILogger<GdprController> logger)
    {
        _gdprService = gdprService ?? throw new ArgumentNullException(nameof(gdprService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("anonymize/{employeeId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AnonymizeEmployeeAsync(string employeeId)
    {
        _logger.LogInformation(
            "Demande d'anonymisation RGPD pour l'employe {EmployeeId}.", employeeId);

        await _gdprService.AnonymizeEmployeeDataAsync(employeeId);
        return NoContent();
    }

    [HttpGet("export/{employeeId}")]
    [ProducesResponseType(typeof(EmployeeTrainingDataExportDto), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<EmployeeTrainingDataExportDto>> ExportEmployeeDataAsync(string employeeId)
    {
        _logger.LogInformation(
            "Export RGPD (Art. 20) des donnees de formation pour l'employe {EmployeeId}.", employeeId);

        var export = await _gdprService.ExportEmployeeTrainingDataAsync(employeeId);
        return Ok(export);
    }

    [HttpGet("retention-status")]
    [ProducesResponseType(typeof(DataRetentionReportDto), 200)]
    public async Task<ActionResult<DataRetentionReportDto>> GetRetentionStatusAsync()
    {
        _logger.LogInformation("Recuperation du rapport de retention des donnees RGPD.");

        var report = await _gdprService.GetDataRetentionStatusAsync();
        return Ok(report);
    }
}
