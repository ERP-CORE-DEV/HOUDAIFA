using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.SkillDevelopment.DTOs.FormationExecution.TrainingAction;
using Training.SkillDevelopment.Services.FormationExecution.TrainingAction;

namespace Training.SkillDevelopment.Controllers.FormationExecution.TrainingAction;

[Authorize]
[ApiController]
[Route("api/training-sessions")]
[Produces("application/json")]
public sealed class TrainingSessionController : ControllerBase
{
    private readonly ITrainingSessionService _service;
    private readonly ILogger<TrainingSessionController> _logger;

    public TrainingSessionController(
        ITrainingSessionService service,
        ILogger<TrainingSessionController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TrainingSessionDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TrainingSessionDto>> GetById(string id)
    {
        try
        {
            var session = await _service.GetByIdAsync(id);
            if (session is null)
            {
                _logger.LogWarning("Session de formation {SessionId} introuvable.", id);
                return NotFound(new { error = "NOT_FOUND", message = "Session de formation introuvable." });
            }

            return Ok(TrainingSessionDto.FromDomain(session));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation de la session {SessionId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpGet("by-action/{actionId}")]
    [ProducesResponseType(typeof(IReadOnlyList<TrainingSessionDto>), 200)]
    public async Task<ActionResult<IReadOnlyList<TrainingSessionDto>>> GetByActionId(string actionId)
    {
        try
        {
            var sessions = await _service.GetByActionIdAsync(actionId);
            return Ok(sessions.Select(TrainingSessionDto.FromDomain).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation des sessions pour l'action {ActionId}.", actionId);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpGet("by-date-range")]
    [ProducesResponseType(typeof(IReadOnlyList<TrainingSessionDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<IReadOnlyList<TrainingSessionDto>>> GetByDateRange(
        [FromQuery] DateTime start,
        [FromQuery] DateTime end)
    {
        try
        {
            if (start >= end)
                return BadRequest(new { error = "VALIDATION_ERROR", message = "La date de debut doit etre anterieure a la date de fin." });

            var sessions = await _service.GetByDateRangeAsync(start, end);
            return Ok(sessions.Select(TrainingSessionDto.FromDomain).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation des sessions par plage de dates.");
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(TrainingSessionDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<TrainingSessionDto>> Create([FromBody] TrainingSessionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var session = dto.ToDomain();
            var created = await _service.CreateAsync(session);
            _logger.LogInformation("Session de formation {SessionId} creee.", created.Id);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, TrainingSessionDto.FromDomain(created));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation echouee lors de la creation de la session.");
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operation invalide lors de la creation de la session.");
            return BadRequest(new { error = "BUSINESS_RULE_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la creation de la session de formation.");
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TrainingSessionDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TrainingSessionDto>> Update(string id, [FromBody] TrainingSessionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.Id = id;
            var session = dto.ToDomain();
            var updated = await _service.UpdateAsync(session);

            return Ok(TrainingSessionDto.FromDomain(updated));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "NOT_FOUND", message = "Session de formation introuvable." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise a jour de la session {SessionId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> Delete(string id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "NOT_FOUND", message = "Session de formation introuvable." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de la session {SessionId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }
}
