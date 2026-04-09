using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.SkillDevelopment.DTOs.FormationExecution.ELearning;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Services.FormationExecution.ELearning;

namespace Training.SkillDevelopment.Controllers.FormationExecution.ELearning;

[Authorize]
[ApiController]
[Route("api/elearning-courses")]
[Produces("application/json")]
public sealed class ElearningController : ControllerBase
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly IElearningService _service;
    private readonly ILogger<ElearningController> _logger;

    public ElearningController(
        IElearningService service,
        ILogger<ElearningController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ElearningCourseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ElearningCourseDto>> GetById(string id)
    {
        try
        {
            var course = await _service.GetByIdAsync(id);
            if (course is null)
            {
                _logger.LogWarning("Cours e-learning {CourseId} introuvable.", id);
                return NotFound(new { error = "NOT_FOUND", message = "Cours e-learning introuvable." });
            }

            return Ok(ElearningCourseDto.FromDomain(course));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation du cours {CourseId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ElearningCourseDto>), 200)]
    public async Task<ActionResult<PagedResult<ElearningCourseDto>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize)
    {
        try
        {
            var clampedPageSize = Math.Min(pageSize, MaxPageSize);
            var result = await _service.GetPagedAsync(page, clampedPageSize);

            var dtoResult = new PagedResult<ElearningCourseDto>
            {
                Items = result.Items.Select(ElearningCourseDto.FromDomain).ToList().AsReadOnly(),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };

            return Ok(dtoResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recuperation paginee des cours e-learning.");
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ElearningCourseDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ElearningCourseDto>> Create([FromBody] ElearningCourseDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var course = dto.ToDomain();
            var created = await _service.CreateAsync(course);
            _logger.LogInformation("Cours e-learning {CourseId} cree.", created.Id);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ElearningCourseDto.FromDomain(created));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation echouee lors de la creation du cours e-learning.");
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la creation du cours e-learning.");
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ElearningCourseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ElearningCourseDto>> Update(string id, [FromBody] ElearningCourseDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var course = new Models.FormationExecution.ELearning.ElearningCourse
            {
                Id = id,
                Title = dto.Title,
                Description = dto.Description,
                Format = dto.Format,
                ScormPackageId = dto.ScormPackageId,
                DurationMinutes = dto.DurationMinutes,
                Provider = dto.Provider,
                Tags = dto.Tags,
                CompetencyIds = dto.CompetencyIds,
                IsActive = dto.IsActive,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };

            var updated = await _service.UpdateAsync(course);
            return Ok(ElearningCourseDto.FromDomain(updated));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "NOT_FOUND", message = "Cours e-learning introuvable." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise a jour du cours {CourseId}.", id);
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
            return NotFound(new { error = "NOT_FOUND", message = "Cours e-learning introuvable." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du cours {CourseId}.", id);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }

    [HttpPost("{courseId}/track-progress")]
    [ProducesResponseType(typeof(LearnerProgressDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<LearnerProgressDto>> TrackProgress(
        string courseId,
        [FromBody] LearnerProgressDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var progress = dto.ToDomain();
            progress.CourseId = courseId;
            var tracked = await _service.TrackProgressAsync(progress);
            _logger.LogInformation("Progression suivie pour le cours {CourseId}, employe {EmployeeId}.", courseId, dto.EmployeeId);

            return Ok(LearnerProgressDto.FromDomain(tracked));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "NOT_FOUND", message = "Cours e-learning introuvable." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "VALIDATION_ERROR", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du suivi de progression pour le cours {CourseId}.", courseId);
            return StatusCode(500, new { error = "SERVER_ERROR", message = "Erreur interne du serveur." });
        }
    }
}
