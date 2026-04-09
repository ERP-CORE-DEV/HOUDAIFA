using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.SkillDevelopment.DTOs.FinancementConformite.Opco;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Services.FinancementConformite.Opco;

namespace Training.SkillDevelopment.Controllers.FinancementConformite.Opco;

[Authorize]
[ApiController]
[Route("api/opcos")]
[Produces("application/json")]
public sealed class OpcoController : ControllerBase
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly IOpcoService _service;
    private readonly ILogger<OpcoController> _logger;

    public OpcoController(
        IOpcoService service,
        ILogger<OpcoController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OpcoDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<OpcoDto>> GetById(string id, CancellationToken cancellationToken)
    {
        try
        {
            var opco = await _service.GetByIdAsync(id, cancellationToken);
            if (opco is null)
            {
                _logger.LogWarning("OPCO {OpcoId} introuvable.", id);
                return NotFound(new { error = "NOT_FOUND", message = "OPCO introuvable." });
            }

            return Ok(OpcoDto.FromDomain(opco));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation de l'OPCO {OpcoId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OpcoDto>), 200)]
    public async Task<ActionResult<PagedResult<OpcoDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var clampedPageSize = Math.Min(pageSize, MaxPageSize);
            var result = await _service.GetAllAsync(page, clampedPageSize, cancellationToken);

            var dtoResult = new PagedResult<OpcoDto>
            {
                Items = result.Items.Select(OpcoDto.FromDomain).ToList().AsReadOnly(),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };

            return Ok(dtoResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation de la liste des OPCOs.");
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(OpcoDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<OpcoDto>> Create([FromBody] OpcoDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var opco = dto.ToDomain();
            var created = await _service.CreateAsync(opco, cancellationToken);
            _logger.LogInformation("OPCO {OpcoId} cree.", created.Id);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, OpcoDto.FromDomain(created));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation echouee lors de la creation de l'OPCO.");
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operation invalide lors de la creation de l'OPCO.");
            return BadRequest(new { error = "BUSINESS_RULE_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la creation de l'OPCO.");
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(OpcoDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<OpcoDto>> Update(string id, [FromBody] OpcoDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.Id = id;
            var opco = dto.ToDomain();
            var updated = await _service.UpdateAsync(opco, cancellationToken);

            return Ok(OpcoDto.FromDomain(updated));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "NOT_FOUND", message = "OPCO introuvable." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise a jour de l'OPCO {OpcoId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        try
        {
            await _service.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "NOT_FOUND", message = "OPCO introuvable." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de l'OPCO {OpcoId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }
}
