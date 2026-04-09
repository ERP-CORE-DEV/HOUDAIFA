using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.SkillDevelopment.DTOs.FormationExecution.Evaluation;
using Training.SkillDevelopment.Services.FormationExecution.Evaluation;

namespace Training.SkillDevelopment.Controllers.FormationExecution.Evaluation;

[Authorize]
[ApiController]
[Route("api/training-evaluations")]
[Produces("application/json")]
public sealed class TrainingEvaluationController : ControllerBase
{
    private readonly ITrainingEvaluationService _service;
    private readonly ILogger<TrainingEvaluationController> _logger;

    public TrainingEvaluationController(
        ITrainingEvaluationService service,
        ILogger<TrainingEvaluationController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TrainingEvaluationDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TrainingEvaluationDto>> GetById(string id)
    {
        try
        {
            var evaluation = await _service.GetByIdAsync(id);
            if (evaluation is null)
            {
                _logger.LogWarning("Evaluation de formation {EvaluationId} introuvable.", id);
                return NotFound(new { error = "NOT_FOUND", message = "Evaluation de formation introuvable." });
            }

            return Ok(TrainingEvaluationDto.FromDomain(evaluation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation de l'evaluation {EvaluationId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpGet("by-session/{sessionId}")]
    [ProducesResponseType(typeof(IReadOnlyList<TrainingEvaluationDto>), 200)]
    public async Task<ActionResult<IReadOnlyList<TrainingEvaluationDto>>> GetBySessionId(string sessionId)
    {
        try
        {
            var evaluations = await _service.GetBySessionIdAsync(sessionId);
            return Ok(evaluations.Select(TrainingEvaluationDto.FromDomain).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation des evaluations pour la session {SessionId}.", sessionId);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpGet("average/{actionId}")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<ActionResult> GetAverageScore(string actionId)
    {
        try
        {
            var average = await _service.GetAverageScoreAsync(actionId);
            return Ok(new { actionId, averageScore = average });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du calcul de la note moyenne pour l'action {ActionId}.", actionId);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(TrainingEvaluationDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<TrainingEvaluationDto>> Create([FromBody] TrainingEvaluationDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var evaluation = dto.ToDomain();
            var created = await _service.CreateAsync(evaluation);
            _logger.LogInformation("Evaluation de formation {EvaluationId} creee.", created.Id);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, TrainingEvaluationDto.FromDomain(created));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation echouee lors de la creation de l'evaluation.");
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operation invalide lors de la creation de l'evaluation.");
            return BadRequest(new { error = "BUSINESS_RULE_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la creation de l'evaluation de formation.");
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TrainingEvaluationDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TrainingEvaluationDto>> Update(string id, [FromBody] TrainingEvaluationDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.Id = id;
            var evaluation = dto.ToDomain();
            var updated = await _service.UpdateAsync(evaluation);

            return Ok(TrainingEvaluationDto.FromDomain(updated));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "NOT_FOUND", message = "Evaluation de formation introuvable." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise a jour de l'evaluation {EvaluationId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }
}
