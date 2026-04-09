using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.SkillDevelopment.DTOs.CertificationEcosysteme.Certification;
using Training.SkillDevelopment.Services.CertificationEcosysteme.Certification;

namespace Training.SkillDevelopment.Controllers.CertificationEcosysteme.Certification;

[Authorize]
[ApiController]
[Route("api/vae-projects")]
[Produces("application/json")]
public sealed class VaeController : ControllerBase
{
    private readonly IVaeProjectService _service;
    private readonly ILogger<VaeController> _logger;

    public VaeController(
        IVaeProjectService service,
        ILogger<VaeController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(VaeProjectDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<VaeProjectDto>> GetById(string id)
    {
        try
        {
            var project = await _service.GetByIdAsync(id);
            if (project is null)
            {
                _logger.LogWarning("Projet VAE {ProjectId} introuvable.", id);
                return NotFound(new { error = "NOT_FOUND", message = "Projet VAE introuvable." });
            }

            return Ok(VaeProjectDto.FromDomain(project));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation du projet VAE {ProjectId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpGet("by-employee/{employeeId}")]
    [ProducesResponseType(typeof(IReadOnlyList<VaeProjectDto>), 200)]
    public async Task<ActionResult<IReadOnlyList<VaeProjectDto>>> GetByEmployeeId(string employeeId)
    {
        try
        {
            var projects = await _service.GetByEmployeeIdAsync(employeeId);
            return Ok(projects.Select(VaeProjectDto.FromDomain).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation des projets VAE pour l'employe {EmployeeId}.", employeeId);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(VaeProjectDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<VaeProjectDto>> Create([FromBody] VaeProjectDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var project = dto.ToDomain();
            var created = await _service.CreateAsync(project);
            _logger.LogInformation("Projet VAE {ProjectId} cree.", created.Id);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, VaeProjectDto.FromDomain(created));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation echouee lors de la creation du projet VAE.");
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operation invalide lors de la creation du projet VAE.");
            return BadRequest(new { error = "BUSINESS_RULE_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la creation du projet VAE.");
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(VaeProjectDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<VaeProjectDto>> Update(string id, [FromBody] VaeProjectDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.Id = id;
            var project = dto.ToDomain();
            var updated = await _service.UpdateAsync(project);

            return Ok(VaeProjectDto.FromDomain(updated));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "NOT_FOUND", message = "Projet VAE introuvable." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise a jour du projet VAE {ProjectId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpPost("{id}/advance-phase")]
    [ProducesResponseType(typeof(VaeProjectDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<VaeProjectDto>> AdvancePhase(string id)
    {
        try
        {
            var advanced = await _service.AdvancePhaseAsync(id);
            _logger.LogInformation("Phase du projet VAE {ProjectId} avancee.", id);
            return Ok(VaeProjectDto.FromDomain(advanced));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "NOT_FOUND", message = "Projet VAE introuvable." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "BUSINESS_RULE_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'avancement de phase du projet VAE {ProjectId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }
}
