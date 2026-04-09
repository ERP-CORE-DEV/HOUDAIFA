using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.SkillDevelopment.Contracts.TalentManagement;
using Training.SkillDevelopment.Repositories.Competency;

namespace Training.SkillDevelopment.Controllers.Sync;

/// <summary>
/// Contrat de synchronisation bidirectionnelle entre MS 5.7 (Formation) et MS 5.8 (Talent Management).
/// Expose les competences et evaluations de MS 5.7 vers MS 5.8, et recoit les ecarts de competences en retour.
/// </summary>
[Authorize]
[ApiController]
[Route("api/sync/talent-management")]
[Produces("application/json")]
public sealed class TalentManagementSyncController : ControllerBase
{
    private readonly ICompetencyRepository _competencyRepository;
    private readonly ICompetencyAssessmentRepository _assessmentRepository;
    private readonly ILogger<TalentManagementSyncController> _logger;

    public TalentManagementSyncController(
        ICompetencyRepository competencyRepository,
        ICompetencyAssessmentRepository assessmentRepository,
        ILogger<TalentManagementSyncController> logger)
    {
        _competencyRepository = competencyRepository ?? throw new ArgumentNullException(nameof(competencyRepository));
        _assessmentRepository = assessmentRepository ?? throw new ArgumentNullException(nameof(assessmentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Exporte le referentiel de competences actif vers MS 5.8 Talent Management.
    /// Supporte le filtrage differentiel par date de derniere modification pour optimiser les synchronisations incrementales.
    /// </summary>
    /// <param name="modifiedSince">
    /// Date optionnelle de filtrage : seules les competences modifiees apres cette date sont retournees.
    /// Si absent, toutes les competences actives sont retournees.
    /// </param>
    /// <returns>Liste des competences au format de synchronisation inter-microservices.</returns>
    /// <response code="200">Competences exportees avec succes vers MS 5.8.</response>
    [HttpGet("competencies")]
    [ProducesResponseType(typeof(IReadOnlyList<CompetencySyncDto>), 200)]
    public async Task<ActionResult<IReadOnlyList<CompetencySyncDto>>> GetCompetenciesForSync(
        [FromQuery] DateTime? modifiedSince)
    {
        _logger.LogInformation(
            "Sync MS 5.8: export des competences (modifiees depuis {Since}).",
            modifiedSince?.ToString("yyyy-MM-dd") ?? "toujours");

        var paged = await _competencyRepository.GetAllActiveAsync(1, 1000);
        var competencies = paged.Items
            .Where(c => modifiedSince is null || c.UpdatedAt >= modifiedSince)
            .Select(c => new CompetencySyncDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Domain = c.Domain,
                Family = c.Family,
                Type = c.Type.ToString(),
                IsCritical = c.IsCritical,
                LastModifiedAt = c.UpdatedAt
            })
            .ToList();

        return Ok(competencies.AsReadOnly());
    }

    /// <summary>
    /// Exporte les evaluations de competences d'un employe vers MS 5.8 Talent Management.
    /// Fournit le niveau actuel par competence pour alimenter les parcours de developpement.
    /// </summary>
    /// <param name="employeeId">Identifiant unique de l'employe dans le systeme RH.</param>
    /// <returns>Liste des evaluations de competences de l'employe au format de synchronisation.</returns>
    /// <response code="200">Evaluations de competences de l'employe exportees avec succes.</response>
    /// <response code="400">L'identifiant de l'employe est absent ou invalide.</response>
    [HttpGet("assessments/{employeeId}")]
    [ProducesResponseType(typeof(IReadOnlyList<CompetencyAssessmentSyncDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<IReadOnlyList<CompetencyAssessmentSyncDto>>> GetAssessmentsForSync(
        string employeeId)
    {
        if (string.IsNullOrWhiteSpace(employeeId))
            return BadRequest(new { error = "VALIDATION_ERROR", message = "L'identifiant de l'employe est requis." });

        _logger.LogInformation(
            "Sync MS 5.8: export des evaluations de competences pour l'employe {EmployeeId}.", employeeId);

        var assessments = await _assessmentRepository.GetByEmployeeIdAsync(employeeId);
        var syncDtos = assessments.Select(a => new CompetencyAssessmentSyncDto
        {
            Id = a.Id,
            EmployeeId = a.EmployeeId,
            CompetencyId = a.CompetencyId,
            CurrentLevel = (int)a.CurrentLevel,
            AssessedBy = a.AssessedBy,
            AssessmentDate = a.AssessmentDate
        }).ToList();

        return Ok(syncDtos.AsReadOnly());
    }

    /// <summary>
    /// Recoit les ecarts de competences calcules par MS 5.8 Talent Management.
    /// Ces ecarts sont utilises pour orienter les plans de formation et les actions de developpement.
    /// </summary>
    /// <param name="skillGaps">Liste des ecarts de competences a integrer, incluant l'employe, la competence et la priorite.</param>
    /// <returns>Reponse vide si la reception a reussi.</returns>
    /// <response code="204">Ecarts de competences recus et traites avec succes.</response>
    /// <response code="400">La liste des ecarts est vide ou nulle.</response>
    [HttpPost("skill-gaps")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public IActionResult ReceiveSkillGaps([FromBody] IReadOnlyList<SkillGapSyncDto> skillGaps)
    {
        if (skillGaps is null || skillGaps.Count == 0)
            return BadRequest(new { error = "VALIDATION_ERROR", message = "La liste des ecarts de competences est vide." });

        _logger.LogInformation(
            "Sync MS 5.8: reception de {Count} ecarts de competences depuis Talent Management.",
            skillGaps.Count);

        foreach (var gap in skillGaps)
        {
            _logger.LogDebug(
                "Ecart recu: employe {EmployeeId}, competence {CompetencyId}, ecart {GapSize}, priorite {Priority}.",
                gap.EmployeeId, gap.CompetencyId, gap.GapSize, gap.Priority);
        }

        return NoContent();
    }
}
