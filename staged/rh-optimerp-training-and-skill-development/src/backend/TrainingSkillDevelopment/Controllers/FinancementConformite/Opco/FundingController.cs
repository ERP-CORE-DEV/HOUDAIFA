using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.SkillDevelopment.DTOs.FinancementConformite.Opco;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FinancementConformite.Opco;
using Training.SkillDevelopment.Services.FinancementConformite.Opco;

namespace Training.SkillDevelopment.Controllers.FinancementConformite.Opco;

[Authorize]
[ApiController]
[Route("api/funding-applications")]
[Produces("application/json")]
public sealed class FundingController : ControllerBase
{
    private readonly IFundingApplicationService _service;
    private readonly ILogger<FundingController> _logger;

    public FundingController(
        IFundingApplicationService service,
        ILogger<FundingController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FundingApplicationDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<FundingApplicationDto>> GetById(string id, CancellationToken cancellationToken)
    {
        try
        {
            var application = await _service.GetByIdAsync(id, cancellationToken);
            if (application is null)
            {
                _logger.LogWarning("Demande de financement {ApplicationId} introuvable.", id);
                return NotFound(new { error = "NOT_FOUND", message = "Demande de financement introuvable." });
            }

            return Ok(FundingApplicationDto.FromDomain(application));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation de la demande {ApplicationId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(FundingApplicationDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<FundingApplicationDto>> Create([FromBody] FundingApplicationDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var application = dto.ToDomain();
            var created = await _service.SubmitAsync(application, cancellationToken);
            _logger.LogInformation("Demande de financement {ApplicationId} creee.", created.Id);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, FundingApplicationDto.FromDomain(created));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation echouee lors de la creation de la demande de financement.");
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operation invalide lors de la creation de la demande.");
            return BadRequest(new { error = "BUSINESS_RULE_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la creation de la demande de financement.");
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(FundingApplicationDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<FundingApplicationDto>> Update(string id, [FromBody] FundingApplicationDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.Id = id;
            var application = dto.ToDomain();
            var updated = await _service.UpdateAsync(application, cancellationToken);

            return Ok(FundingApplicationDto.FromDomain(updated));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "NOT_FOUND", message = "Demande de financement introuvable." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise a jour de la demande {ApplicationId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpPost("{id}/submit")]
    [ProducesResponseType(typeof(FundingApplicationDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<FundingApplicationDto>> Submit(string id, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _service.GetByIdAsync(id, cancellationToken);
            if (existing is null)
                return NotFound(new { error = "NOT_FOUND", message = "Demande de financement introuvable." });

            existing.Status = FundingStatus.Submitted;
            var submitted = await _service.UpdateAsync(existing, cancellationToken);
            _logger.LogInformation("Demande de financement {ApplicationId} soumise.", id);

            return Ok(FundingApplicationDto.FromDomain(submitted));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "BUSINESS_RULE_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la soumission de la demande {ApplicationId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpPost("{id}/approve")]
    [ProducesResponseType(typeof(FundingApplicationDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<FundingApplicationDto>> Approve(
        string id,
        [FromBody] ApproveFundingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request.GrantedAmount <= 0)
                return BadRequest(new { error = "VALIDATION_ERROR", message = "Le montant accorde doit etre superieur a zero." });

            var approved = await _service.ApproveAsync(id, request.GrantedAmount, cancellationToken);
            _logger.LogInformation("Demande de financement {ApplicationId} approuvee.", id);

            return Ok(FundingApplicationDto.FromDomain(approved));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "NOT_FOUND", message = "Demande de financement introuvable." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "BUSINESS_RULE_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'approbation de la demande {ApplicationId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpPost("{id}/reject")]
    [ProducesResponseType(typeof(FundingApplicationDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<FundingApplicationDto>> Reject(
        string id,
        [FromBody] RejectFundingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
                return BadRequest(new { error = "VALIDATION_ERROR", message = "Le motif de rejet est obligatoire." });

            var rejected = await _service.RejectAsync(id, request.Reason, cancellationToken);
            _logger.LogInformation("Demande de financement {ApplicationId} rejetee.", id);

            return Ok(FundingApplicationDto.FromDomain(rejected));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "NOT_FOUND", message = "Demande de financement introuvable." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "BUSINESS_RULE_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du rejet de la demande {ApplicationId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }
}

public sealed class ApproveFundingRequest
{
    public decimal GrantedAmount { get; set; }
}

public sealed class RejectFundingRequest
{
    public string Reason { get; set; } = string.Empty;
}
