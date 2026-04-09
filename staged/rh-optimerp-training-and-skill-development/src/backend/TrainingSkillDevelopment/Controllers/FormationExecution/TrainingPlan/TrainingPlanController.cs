using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.DTOs.FormationExecution.TrainingPlan;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Services.FormationExecution.TrainingPlan;

namespace Training.SkillDevelopment.Controllers.FormationExecution.TrainingPlan;

/// <summary>
/// Gestion des plans de formation annuels et pluriannuels de l'entreprise.
/// </summary>
[Authorize]
[ApiController]
[Route("api/training-plans")]
[Produces("application/json")]
public sealed class TrainingPlanController : ControllerBase
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly ITrainingPlanService _service;
    private readonly ILogger<TrainingPlanController> _logger;

    public TrainingPlanController(
        ITrainingPlanService service,
        ILogger<TrainingPlanController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Recupere un plan de formation par son identifiant unique.</summary>
    /// <param name="id">Identifiant unique du plan de formation.</param>
    /// <returns>Le plan de formation correspondant a l'identifiant.</returns>
    /// <response code="200">Plan de formation retourne avec succes.</response>
    /// <response code="404">Aucun plan de formation trouve pour cet identifiant.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TrainingPlanDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TrainingPlanDto>> GetById(string id)
    {
        try
        {
            var plan = await _service.GetByIdAsync(id);
            if (plan is null)
            {
                _logger.LogWarning("Plan de formation {PlanId} introuvable.", id);
                return NotFound(new { error = "NOT_FOUND", message = "Plan de formation introuvable." });
            }

            return Ok(TrainingPlanDto.FromDomain(plan));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation du plan {PlanId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    /// <summary>Recupere la liste paginee des plans de formation.</summary>
    /// <param name="page">Numero de la page (commence a 1).</param>
    /// <param name="pageSize">Nombre d'elements par page (max 100, defaut 20).</param>
    /// <returns>Une page de plans de formation avec les informations de pagination.</returns>
    /// <response code="200">Liste paginee des plans de formation retournee avec succes.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TrainingPlanDto>), 200)]
    public async Task<ActionResult<PagedResult<TrainingPlanDto>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize)
    {
        try
        {
            var clampedPageSize = Math.Min(pageSize, MaxPageSize);
            var result = await _service.GetPagedAsync(page, clampedPageSize);

            var dtoResult = new PagedResult<TrainingPlanDto>
            {
                Items = result.Items.Select(TrainingPlanDto.FromDomain).ToList().AsReadOnly(),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };

            return Ok(dtoResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation paginee des plans.");
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    /// <summary>Cree un nouveau plan de formation.</summary>
    /// <param name="dto">Donnees du plan de formation a creer.</param>
    /// <returns>Le plan de formation cree avec son identifiant genere.</returns>
    /// <response code="201">Plan de formation cree avec succes.</response>
    /// <response code="400">Donnees invalides ou regles metier non respectees.</response>
    [HttpPost]
    [ProducesResponseType(typeof(TrainingPlanDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<TrainingPlanDto>> Create([FromBody] TrainingPlanDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Donnees invalides pour la creation d'un plan de formation.");
                return BadRequest(ModelState);
            }

            var plan = dto.ToDomain();
            var created = await _service.CreateAsync(plan);
            _logger.LogInformation("Plan de formation {PlanId} cree.", created.Id);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, TrainingPlanDto.FromDomain(created));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation echouee lors de la creation du plan.");
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operation invalide lors de la creation du plan.");
            return BadRequest(new { error = "BUSINESS_RULE_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la creation du plan de formation.");
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    /// <summary>Met a jour un plan de formation existant.</summary>
    /// <param name="id">Identifiant unique du plan de formation a mettre a jour.</param>
    /// <param name="dto">Nouvelles donnees du plan de formation.</param>
    /// <returns>Le plan de formation mis a jour.</returns>
    /// <response code="200">Plan de formation mis a jour avec succes.</response>
    /// <response code="400">Donnees invalides ou regles metier non respectees.</response>
    /// <response code="404">Plan de formation introuvable pour cet identifiant.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TrainingPlanDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TrainingPlanDto>> Update(string id, [FromBody] TrainingPlanDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.Id = id;
            var plan = dto.ToDomain();
            var updated = await _service.UpdateAsync(plan);

            return Ok(TrainingPlanDto.FromDomain(updated));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "NOT_FOUND", message = "Plan de formation introuvable." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "BUSINESS_RULE_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise a jour du plan {PlanId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    /// <summary>Supprime un plan de formation par son identifiant.</summary>
    /// <param name="id">Identifiant unique du plan de formation a supprimer.</param>
    /// <returns>Reponse vide si la suppression a reussi.</returns>
    /// <response code="204">Plan de formation supprime avec succes.</response>
    /// <response code="404">Plan de formation introuvable pour cet identifiant.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> Delete(string id)
    {
        try
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { error = "NOT_FOUND", message = "Plan de formation introuvable." });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du plan {PlanId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    /// <summary>Approuve un plan de formation en attente de validation.</summary>
    /// <param name="id">Identifiant unique du plan de formation a approuver.</param>
    /// <param name="request">Requete contenant l'identifiant de l'approbateur.</param>
    /// <returns>Le plan de formation avec le statut approuve.</returns>
    /// <response code="200">Plan de formation approuve avec succes.</response>
    /// <response code="400">Donnees invalides ou le plan n'est pas dans un etat approvable.</response>
    /// <response code="404">Plan de formation introuvable pour cet identifiant.</response>
    [HttpPost("{id}/approve")]
    [ProducesResponseType(typeof(TrainingPlanDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TrainingPlanDto>> Approve(string id, [FromBody] ApproveTrainingPlanRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.ApprovedBy))
                return BadRequest(new { error = "VALIDATION_ERROR", message = "L'identifiant de l'approbateur est requis." });

            var approved = await _service.ApproveAsync(id, request.ApprovedBy);
            return Ok(TrainingPlanDto.FromDomain(approved));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "NOT_FOUND", message = "Plan de formation introuvable." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "BUSINESS_RULE_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'approbation du plan {PlanId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }
}

/// <summary>Corps de la requete d'approbation d'un plan de formation.</summary>
public sealed class ApproveTrainingPlanRequest
{
    /// <summary>Identifiant de l'utilisateur qui approuve le plan.</summary>
    public string ApprovedBy { get; set; } = string.Empty;
}
