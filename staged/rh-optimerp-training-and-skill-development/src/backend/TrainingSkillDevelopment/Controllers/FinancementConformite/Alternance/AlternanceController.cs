using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.SkillDevelopment.DTOs.FinancementConformite.Alternance;
using Training.SkillDevelopment.Services.FinancementConformite.Alternance;

namespace Training.SkillDevelopment.Controllers.FinancementConformite.Alternance;

[Authorize]
[ApiController]
[Route("api/alternance-contracts")]
[Produces("application/json")]
public sealed class AlternanceController : ControllerBase
{
    private readonly IAlternanceService _service;
    private readonly ILogger<AlternanceController> _logger;

    public AlternanceController(
        IAlternanceService service,
        ILogger<AlternanceController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AlternanceContractDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<AlternanceContractDto>> GetById(string id)
    {
        try
        {
            var contract = await _service.GetByIdAsync(id);
            if (contract is null)
            {
                _logger.LogWarning("Contrat d'alternance {ContractId} introuvable.", id);
                return NotFound(new { error = "NOT_FOUND", message = "Contrat d'alternance introuvable." });
            }

            return Ok(AlternanceContractDto.FromDomain(contract));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation du contrat {ContractId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpGet("by-employee/{employeeId}")]
    [ProducesResponseType(typeof(IReadOnlyList<AlternanceContractDto>), 200)]
    public async Task<ActionResult<IReadOnlyList<AlternanceContractDto>>> GetByEmployeeId(string employeeId)
    {
        try
        {
            var contracts = await _service.GetByEmployeeIdAsync(employeeId);
            return Ok(contracts.Select(AlternanceContractDto.FromDomain).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation des contrats pour l'employe {EmployeeId}.", employeeId);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpGet("calculate-remuneration")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> CalculateRemuneration([FromQuery] string contractId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(contractId))
                return BadRequest(new { error = "VALIDATION_ERROR", message = "L'identifiant du contrat est requis." });

            var remuneration = await _service.CalculateRemunerationAsync(contractId);
            return Ok(new { contractId, remunerationAmount = remuneration });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "NOT_FOUND", message = "Contrat d'alternance introuvable." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du calcul de la remuneration pour le contrat {ContractId}.", contractId);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(AlternanceContractDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<AlternanceContractDto>> Create([FromBody] AlternanceContractDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var contract = dto.ToDomain();
            var created = await _service.CreateAsync(contract);
            _logger.LogInformation("Contrat d'alternance {ContractId} cree.", created.Id);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, AlternanceContractDto.FromDomain(created));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation echouee lors de la creation du contrat d'alternance.");
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operation invalide lors de la creation du contrat d'alternance.");
            return BadRequest(new { error = "BUSINESS_RULE_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la creation du contrat d'alternance.");
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AlternanceContractDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<AlternanceContractDto>> Update(string id, [FromBody] AlternanceContractDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.Id = id;
            var contract = dto.ToDomain();
            var updated = await _service.UpdateAsync(contract);

            return Ok(AlternanceContractDto.FromDomain(updated));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "NOT_FOUND", message = "Contrat d'alternance introuvable." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise a jour du contrat {ContractId}.", id);
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
            return NotFound(new { error = "NOT_FOUND", message = "Contrat d'alternance introuvable." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du contrat {ContractId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }
}
