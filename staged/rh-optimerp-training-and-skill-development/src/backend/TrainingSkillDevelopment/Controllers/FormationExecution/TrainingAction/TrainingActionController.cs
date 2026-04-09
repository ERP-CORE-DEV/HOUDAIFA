using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.SkillDevelopment.DTOs.FormationExecution.TrainingAction;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Services.FormationExecution.TrainingAction;

namespace Training.SkillDevelopment.Controllers.FormationExecution.TrainingAction;

[Authorize]
[ApiController]
[Route("api/training-actions")]
[Produces("application/json")]
public sealed class TrainingActionController : ControllerBase
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly ITrainingActionService _service;
    private readonly ILogger<TrainingActionController> _logger;

    public TrainingActionController(
        ITrainingActionService service,
        ILogger<TrainingActionController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TrainingActionDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TrainingActionDto>> GetById(string id)
    {
        try
        {
            var action = await _service.GetByIdAsync(id);
            if (action is null)
            {
                _logger.LogWarning("Action de formation {ActionId} introuvable.", id);
                return NotFound(new { error = "NOT_FOUND", message = "Action de formation introuvable." });
            }

            return Ok(TrainingActionDto.FromDomain(action));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation de l'action {ActionId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TrainingActionDto>), 200)]
    public async Task<ActionResult<PagedResult<TrainingActionDto>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize)
    {
        try
        {
            var clampedPageSize = Math.Min(pageSize, MaxPageSize);
            var result = await _service.GetPagedAsync(page, clampedPageSize);

            var dtoResult = new PagedResult<TrainingActionDto>
            {
                Items = result.Items.Select(TrainingActionDto.FromDomain).ToList().AsReadOnly(),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };

            return Ok(dtoResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation paginee des actions de formation.");
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpGet("by-plan/{planId}")]
    [ProducesResponseType(typeof(IReadOnlyList<TrainingActionDto>), 200)]
    public async Task<ActionResult<IReadOnlyList<TrainingActionDto>>> GetByPlanId(string planId)
    {
        try
        {
            var actions = await _service.GetByPlanIdAsync(planId);
            return Ok(actions.Select(TrainingActionDto.FromDomain).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation des actions pour le plan {PlanId}.", planId);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(TrainingActionDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<TrainingActionDto>> Create([FromBody] TrainingActionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var action = dto.ToDomain();
            var created = await _service.CreateAsync(action);
            _logger.LogInformation("Action de formation {ActionId} creee.", created.Id);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, TrainingActionDto.FromDomain(created));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation echouee lors de la creation de l'action.");
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operation invalide lors de la creation de l'action.");
            return BadRequest(new { error = "BUSINESS_RULE_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la creation de l'action de formation.");
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TrainingActionDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TrainingActionDto>> Update(string id, [FromBody] TrainingActionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.Id = id;
            var action = dto.ToDomain();
            var updated = await _service.UpdateAsync(action);

            return Ok(TrainingActionDto.FromDomain(updated));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "NOT_FOUND", message = "Action de formation introuvable." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise a jour de l'action {ActionId}.", id);
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
            return NotFound(new { error = "NOT_FOUND", message = "Action de formation introuvable." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de l'action {ActionId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }
}
