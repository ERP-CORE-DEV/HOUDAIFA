using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.SkillDevelopment.DTOs.CertificationEcosysteme.Provider;
using Training.SkillDevelopment.Services.CertificationEcosysteme.Provider;

namespace Training.SkillDevelopment.Controllers.CertificationEcosysteme.Provider;

[Authorize]
[ApiController]
[Route("api/training-providers")]
[Produces("application/json")]
public sealed class TrainingProviderController : ControllerBase
{
    private readonly ITrainingProviderService _service;
    private readonly ILogger<TrainingProviderController> _logger;

    public TrainingProviderController(
        ITrainingProviderService service,
        ILogger<TrainingProviderController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TrainingProviderDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TrainingProviderDto>> GetById(string id)
    {
        try
        {
            var provider = await _service.GetByIdAsync(id);
            if (provider is null)
            {
                _logger.LogWarning("Organisme de formation {ProviderId} introuvable.", id);
                return NotFound(new { error = "NOT_FOUND", message = "Organisme de formation introuvable." });
            }

            return Ok(TrainingProviderDto.FromDomain(provider));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation de l'organisme {ProviderId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TrainingProviderDto>), 200)]
    public async Task<ActionResult<IReadOnlyList<TrainingProviderDto>>> GetAll()
    {
        try
        {
            var providers = await _service.GetAllAsync();
            return Ok(providers.Select(TrainingProviderDto.FromDomain).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation de la liste des organismes de formation.");
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpGet("qualiopi-certified")]
    [ProducesResponseType(typeof(IReadOnlyList<TrainingProviderDto>), 200)]
    public async Task<ActionResult<IReadOnlyList<TrainingProviderDto>>> GetQualiopiCertified()
    {
        try
        {
            var providers = await _service.GetAllAsync();
            var qualiopiCertified = providers
                .Where(p => p.HasQualiopiCertification)
                .Select(TrainingProviderDto.FromDomain)
                .ToList()
                .AsReadOnly();

            return Ok(qualiopiCertified);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation des organismes certifies Qualiopi.");
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(TrainingProviderDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<TrainingProviderDto>> Create([FromBody] TrainingProviderDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var provider = dto.ToDomain();
            var created = await _service.CreateAsync(provider);
            _logger.LogInformation("Organisme de formation {ProviderId} cree.", created.Id);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, TrainingProviderDto.FromDomain(created));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation echouee lors de la creation de l'organisme de formation.");
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la creation de l'organisme de formation.");
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TrainingProviderDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TrainingProviderDto>> Update(string id, [FromBody] TrainingProviderDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.Id = id;
            var provider = dto.ToDomain();
            var updated = await _service.UpdateAsync(provider);

            return Ok(TrainingProviderDto.FromDomain(updated));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "NOT_FOUND", message = "Organisme de formation introuvable." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise a jour de l'organisme {ProviderId}.", id);
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
            return NotFound(new { error = "NOT_FOUND", message = "Organisme de formation introuvable." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de l'organisme {ProviderId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }
}
