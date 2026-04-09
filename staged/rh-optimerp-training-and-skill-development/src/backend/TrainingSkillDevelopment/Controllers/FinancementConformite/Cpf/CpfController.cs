using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.SkillDevelopment.DTOs.FinancementConformite.Cpf;
using Training.SkillDevelopment.Services.FinancementConformite.Cpf;

namespace Training.SkillDevelopment.Controllers.FinancementConformite.Cpf;

/// <summary>
/// Gestion des comptes CPF (Compte Personnel de Formation) des salaries.
/// Permet la consultation du solde, la mobilisation des droits et l'application des credits annuels
/// conformement au Code du travail francais (articles L6323-1 et suivants).
/// </summary>
[Authorize]
[ApiController]
[Route("api/cpf")]
[Produces("application/json")]
public sealed class CpfController : ControllerBase
{
    private readonly ICpfService _service;
    private readonly ILogger<CpfController> _logger;

    public CpfController(
        ICpfService service,
        ILogger<CpfController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Recupere un compte CPF par son identifiant unique.</summary>
    /// <param name="id">Identifiant unique du compte CPF.</param>
    /// <param name="cancellationToken">Jeton d'annulation de la requete.</param>
    /// <returns>Le compte CPF avec son solde et l'historique des mobilisations.</returns>
    /// <response code="200">Compte CPF retourne avec succes.</response>
    /// <response code="404">Aucun compte CPF trouve pour cet identifiant.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CpfAccountDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CpfAccountDto>> GetById(string id, CancellationToken cancellationToken)
    {
        try
        {
            var account = await _service.GetAccountByIdAsync(id, cancellationToken);
            if (account is null)
            {
                _logger.LogWarning("Compte CPF {AccountId} introuvable.", id);
                return NotFound(new { error = "NOT_FOUND", message = "Compte CPF introuvable." });
            }

            return Ok(CpfAccountDto.FromDomain(account));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation du compte CPF {AccountId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    /// <summary>Recupere le compte CPF associe a un employe par son identifiant RH.</summary>
    /// <param name="employeeId">Identifiant unique de l'employe dans le systeme RH.</param>
    /// <param name="cancellationToken">Jeton d'annulation de la requete.</param>
    /// <returns>Le compte CPF de l'employe avec son solde disponible.</returns>
    /// <response code="200">Compte CPF de l'employe retourne avec succes.</response>
    /// <response code="404">Aucun compte CPF trouve pour cet employe.</response>
    [HttpGet("by-employee/{employeeId}")]
    [ProducesResponseType(typeof(CpfAccountDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CpfAccountDto>> GetByEmployeeId(string employeeId, CancellationToken cancellationToken)
    {
        try
        {
            var account = await _service.GetAccountByEmployeeIdAsync(employeeId, cancellationToken);
            if (account is null)
            {
                _logger.LogWarning("Compte CPF pour l'employe {EmployeeId} introuvable.", employeeId);
                return NotFound(new { error = "NOT_FOUND", message = "Compte CPF introuvable pour cet employe." });
            }

            return Ok(CpfAccountDto.FromDomain(account));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation du compte CPF pour l'employe {EmployeeId}.", employeeId);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    /// <summary>Cree un nouveau compte CPF pour un employe.</summary>
    /// <param name="dto">Donnees du compte CPF a creer, incluant l'identifiant de l'employe et le solde initial.</param>
    /// <param name="cancellationToken">Jeton d'annulation de la requete.</param>
    /// <returns>Le compte CPF cree avec son identifiant genere.</returns>
    /// <response code="201">Compte CPF cree avec succes.</response>
    /// <response code="400">Donnees invalides ou un compte CPF existe deja pour cet employe.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CpfAccountDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<CpfAccountDto>> Create([FromBody] CpfAccountDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var account = dto.ToDomain();
            var created = await _service.CreateAccountAsync(account, cancellationToken);
            _logger.LogInformation("Compte CPF {AccountId} cree pour l'employe {EmployeeId}.", created.Id, created.EmployeeId);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, CpfAccountDto.FromDomain(created));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation echouee lors de la creation du compte CPF.");
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operation invalide lors de la creation du compte CPF.");
            return BadRequest(new { error = "BUSINESS_RULE_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la creation du compte CPF.");
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    /// <summary>Met a jour les informations d'un compte CPF existant.</summary>
    /// <param name="id">Identifiant unique du compte CPF a mettre a jour.</param>
    /// <param name="dto">Nouvelles donnees du compte CPF.</param>
    /// <param name="cancellationToken">Jeton d'annulation de la requete.</param>
    /// <returns>Le compte CPF mis a jour.</returns>
    /// <response code="200">Compte CPF mis a jour avec succes.</response>
    /// <response code="404">Compte CPF introuvable pour cet identifiant.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CpfAccountDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CpfAccountDto>> Update(string id, [FromBody] CpfAccountDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _service.GetAccountByIdAsync(id, cancellationToken);
            if (existing is null)
                return NotFound(new { error = "NOT_FOUND", message = "Compte CPF introuvable." });

            var account = dto.ToDomain();
            var updated = await _service.CreateAccountAsync(account, cancellationToken);

            return Ok(CpfAccountDto.FromDomain(updated));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "NOT_FOUND", message = "Compte CPF introuvable." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise a jour du compte CPF {AccountId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    /// <summary>
    /// Applique le credit annuel CPF reglementaire au compte d'un employe.
    /// Le montant est calcule selon les regles legales en vigueur (500 EUR/an pour un temps plein,
    /// 800 EUR/an pour un travailleur handicape, plafond a 5000 EUR ou 8000 EUR).
    /// </summary>
    /// <param name="id">Identifiant unique du compte CPF sur lequel appliquer le credit.</param>
    /// <param name="cancellationToken">Jeton d'annulation de la requete.</param>
    /// <returns>Le compte CPF avec le nouveau solde apres credit annuel.</returns>
    /// <response code="200">Credit annuel CPF applique avec succes.</response>
    /// <response code="400">Le credit ne peut pas etre applique (plafond atteint ou credit deja applique cette annee).</response>
    /// <response code="404">Compte CPF introuvable pour cet identifiant.</response>
    [HttpPost("{id}/credit-annual")]
    [ProducesResponseType(typeof(CpfAccountDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CpfAccountDto>> ApplyAnnualCredit(string id, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _service.ApplyAnnualCreditAsync(id, cancellationToken);
            _logger.LogInformation("Credit annuel CPF applique au compte {AccountId}.", id);
            return Ok(CpfAccountDto.FromDomain(updated));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "NOT_FOUND", message = "Compte CPF introuvable." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "BUSINESS_RULE_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'application du credit annuel CPF {AccountId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    /// <summary>
    /// Mobilise des droits CPF pour financer une formation eligible.
    /// Verifie que le solde est suffisant et que la formation est eligible CPF
    /// conformement au catalogue Mon Compte Formation.
    /// </summary>
    /// <param name="id">Identifiant unique du compte CPF depuis lequel mobiliser les droits.</param>
    /// <param name="dto">Donnees de la mobilisation incluant le montant et l'identifiant de formation.</param>
    /// <param name="cancellationToken">Jeton d'annulation de la requete.</param>
    /// <returns>La mobilisation CPF creee avec la reference de dossier.</returns>
    /// <response code="200">Mobilisation CPF enregistree avec succes.</response>
    /// <response code="400">Solde insuffisant, formation non eligible CPF, ou donnees invalides.</response>
    /// <response code="404">Compte CPF introuvable pour cet identifiant.</response>
    [HttpPost("{id}/mobilize")]
    [ProducesResponseType(typeof(CpfMobilizationDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CpfMobilizationDto>> Mobilize(string id, [FromBody] CpfMobilizationDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var mobilization = dto.ToDomain();
            mobilization.AccountId = id;
            var result = await _service.MobilizeCpfAsync(mobilization, cancellationToken);
            _logger.LogInformation("Mobilisation CPF creee pour le compte {AccountId}.", id);

            return Ok(CpfMobilizationDto.FromDomain(result));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "NOT_FOUND", message = "Compte CPF introuvable." });
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
            _logger.LogError(ex, "Erreur lors de la mobilisation CPF pour le compte {AccountId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }
}
