using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.SkillDevelopment.DTOs.FinancementConformite.Compliance;
using Training.SkillDevelopment.Services.FinancementConformite.Compliance;

namespace Training.SkillDevelopment.Controllers.FinancementConformite.Compliance;

[Authorize]
[ApiController]
[Route("api/training-obligations")]
[Produces("application/json")]
public sealed class ComplianceController : ControllerBase
{
    private const int DefaultDaysAhead = 90;

    private readonly ITrainingObligationService _service;
    private readonly ILogger<ComplianceController> _logger;

    public ComplianceController(
        ITrainingObligationService service,
        ILogger<ComplianceController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TrainingObligationDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TrainingObligationDto>> GetById(string id)
    {
        try
        {
            var obligation = await _service.GetByIdAsync(id);
            if (obligation is null)
            {
                _logger.LogWarning("Obligation de formation {ObligationId} introuvable.", id);
                return NotFound(new { error = "NOT_FOUND", message = "Obligation de formation introuvable." });
            }

            return Ok(TrainingObligationDto.FromDomain(obligation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation de l'obligation {ObligationId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TrainingObligationDto>), 200)]
    public async Task<ActionResult<IReadOnlyList<TrainingObligationDto>>> GetAll()
    {
        try
        {
            var obligations = await _service.GetAllAsync();
            return Ok(obligations.Select(TrainingObligationDto.FromDomain).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation de la liste des obligations de formation.");
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpGet("expiring")]
    [ProducesResponseType(typeof(IReadOnlyList<TrainingObligationDto>), 200)]
    public async Task<ActionResult<IReadOnlyList<TrainingObligationDto>>> GetExpiring(
        [FromQuery] int daysAhead = DefaultDaysAhead)
    {
        try
        {
            var obligations = await _service.GetExpiringAsync(daysAhead);
            return Ok(obligations.Select(TrainingObligationDto.FromDomain).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation des obligations expirant dans {DaysAhead} jours.", daysAhead);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpGet("risk-assessment")]
    [ProducesResponseType(typeof(IReadOnlyList<ComplianceAlertDto>), 200)]
    public async Task<ActionResult<IReadOnlyList<ComplianceAlertDto>>> GetRiskAssessment()
    {
        try
        {
            var alerts = await _service.GetRiskAssessmentAsync();
            return Ok(alerts.Select(ComplianceAlertDto.FromDomain).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'evaluation des risques de conformite.");
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(TrainingObligationDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<TrainingObligationDto>> Create([FromBody] TrainingObligationDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var obligation = dto.ToDomain();
            var created = await _service.CreateAsync(obligation);
            _logger.LogInformation("Obligation de formation {ObligationId} creee.", created.Id);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, TrainingObligationDto.FromDomain(created));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation echouee lors de la creation de l'obligation.");
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la creation de l'obligation de formation.");
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TrainingObligationDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TrainingObligationDto>> Update(string id, [FromBody] TrainingObligationDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.Id = id;
            var obligation = dto.ToDomain();
            var updated = await _service.UpdateAsync(obligation);

            return Ok(TrainingObligationDto.FromDomain(updated));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "NOT_FOUND", message = "Obligation de formation introuvable." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise a jour de l'obligation {ObligationId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }
}
