using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.SkillDevelopment.DTOs.CertificationEcosysteme.Certification;
using Training.SkillDevelopment.Services.CertificationEcosysteme.Certification;

namespace Training.SkillDevelopment.Controllers.CertificationEcosysteme.Certification;

/// <summary>
/// Gestion des certifications professionnelles RNCP (Repertoire National des Certifications Professionnelles)
/// et RS (Repertoire Specifique). Permet le suivi des certifications obtenues, en cours et expirant.
/// </summary>
[Authorize]
[ApiController]
[Route("api/certifications")]
[Produces("application/json")]
public sealed class CertificationController : ControllerBase
{
    private const int DefaultDaysAhead = 90;

    private readonly ICertificationService _service;
    private readonly ILogger<CertificationController> _logger;

    public CertificationController(
        ICertificationService service,
        ILogger<CertificationController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Recupere une certification RNCP ou RS par son identifiant unique.</summary>
    /// <param name="id">Identifiant unique de la certification.</param>
    /// <returns>La certification avec ses details RNCP/RS et sa date d'expiration.</returns>
    /// <response code="200">Certification retournee avec succes.</response>
    /// <response code="404">Aucune certification trouvee pour cet identifiant.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CertificationRncpDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CertificationRncpDto>> GetById(string id)
    {
        try
        {
            var certification = await _service.GetByIdAsync(id);
            if (certification is null)
            {
                _logger.LogWarning("Certification {CertificationId} introuvable.", id);
                return NotFound(new { error = "NOT_FOUND", message = "Certification introuvable." });
            }

            return Ok(CertificationRncpDto.FromDomain(certification));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation de la certification {CertificationId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    /// <summary>Recupere la liste complete de toutes les certifications enregistrees.</summary>
    /// <returns>Liste de toutes les certifications RNCP et RS disponibles.</returns>
    /// <response code="200">Liste des certifications retournee avec succes.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CertificationRncpDto>), 200)]
    public async Task<ActionResult<IReadOnlyList<CertificationRncpDto>>> GetAll()
    {
        try
        {
            var certifications = await _service.GetAllAsync();
            return Ok(certifications.Select(CertificationRncpDto.FromDomain).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation de la liste des certifications.");
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    /// <summary>
    /// Recupere les certifications dont la date d'expiration approche dans un delai donne.
    /// Permet d'anticiper les renouvellements et d'alerter les RH et les employes concernes.
    /// </summary>
    /// <param name="daysAhead">Nombre de jours avant expiration pour filtrer les certifications (defaut 90 jours).</param>
    /// <returns>Liste des certifications expirant dans le delai specifie.</returns>
    /// <response code="200">Liste des certifications expirant bientot retournee avec succes.</response>
    [HttpGet("expiring")]
    [ProducesResponseType(typeof(IReadOnlyList<CertificationRncpDto>), 200)]
    public async Task<ActionResult<IReadOnlyList<CertificationRncpDto>>> GetExpiring(
        [FromQuery] int daysAhead = DefaultDaysAhead)
    {
        try
        {
            var certifications = await _service.GetExpiringCertificationsAsync(daysAhead);
            return Ok(certifications.Select(CertificationRncpDto.FromDomain).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation des certifications expirant dans {DaysAhead} jours.", daysAhead);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    /// <summary>Enregistre une nouvelle certification professionnelle RNCP ou RS.</summary>
    /// <param name="dto">Donnees de la certification a creer, incluant le code RNCP/RS et la date d'expiration.</param>
    /// <returns>La certification creee avec son identifiant genere.</returns>
    /// <response code="201">Certification creee avec succes.</response>
    /// <response code="400">Donnees invalides, code RNCP/RS incorrect, ou certification deja enregistree.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CertificationRncpDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<CertificationRncpDto>> Create([FromBody] CertificationRncpDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var certification = dto.ToDomain();
            var created = await _service.CreateAsync(certification);
            _logger.LogInformation("Certification {CertificationId} creee.", created.Id);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, CertificationRncpDto.FromDomain(created));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation echouee lors de la creation de la certification.");
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operation invalide lors de la creation de la certification.");
            return BadRequest(new { error = "BUSINESS_RULE_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la creation de la certification.");
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    /// <summary>Met a jour une certification professionnelle existante.</summary>
    /// <param name="id">Identifiant unique de la certification a mettre a jour.</param>
    /// <param name="dto">Nouvelles donnees de la certification.</param>
    /// <returns>La certification mise a jour.</returns>
    /// <response code="200">Certification mise a jour avec succes.</response>
    /// <response code="400">Donnees invalides ou code RNCP/RS incorrect.</response>
    /// <response code="404">Certification introuvable pour cet identifiant.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CertificationRncpDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CertificationRncpDto>> Update(string id, [FromBody] CertificationRncpDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.Id = id;
            var certification = dto.ToDomain();
            var updated = await _service.UpdateAsync(certification);

            return Ok(CertificationRncpDto.FromDomain(updated));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "NOT_FOUND", message = "Certification introuvable." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise a jour de la certification {CertificationId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    /// <summary>Supprime une certification professionnelle par son identifiant.</summary>
    /// <param name="id">Identifiant unique de la certification a supprimer.</param>
    /// <returns>Reponse vide si la suppression a reussi.</returns>
    /// <response code="204">Certification supprimee avec succes.</response>
    /// <response code="404">Certification introuvable pour cet identifiant.</response>
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
            return NotFound(new { error = "NOT_FOUND", message = "Certification introuvable." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de la certification {CertificationId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }
}
