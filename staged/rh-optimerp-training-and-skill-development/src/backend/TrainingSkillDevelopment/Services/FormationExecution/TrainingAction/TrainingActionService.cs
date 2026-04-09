using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Repositories.FormationExecution.TrainingAction;
using TrainingActionEntity = Training.SkillDevelopment.Models.FormationExecution.TrainingAction.TrainingAction;

namespace Training.SkillDevelopment.Services.FormationExecution.TrainingAction;

public sealed class TrainingActionService : ITrainingActionService
{
    private readonly ITrainingActionRepository _repository;
    private readonly ILogger<TrainingActionService> _logger;

    public TrainingActionService(
        ITrainingActionRepository repository,
        ILogger<TrainingActionService> logger)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(logger);
        _repository = repository;
        _logger = logger;
    }

    public async Task<TrainingActionEntity?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant de l'action est obligatoire.", nameof(id));

        return await _repository.GetByIdAsync(id);
    }

    public async Task<PagedResult<TrainingActionEntity>> GetPagedAsync(int page, int pageSize)
    {
        if (page < 1)
            throw new ArgumentException("Le numero de page doit etre superieur a 0.", nameof(page));

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException("La taille de la page doit etre comprise entre 1 et 100.", nameof(pageSize));

        return await _repository.GetPagedAsync(page, pageSize);
    }

    public async Task<IReadOnlyList<TrainingActionEntity>> GetByPlanIdAsync(string planId)
    {
        if (string.IsNullOrWhiteSpace(planId))
            throw new ArgumentException("L'identifiant du plan est obligatoire.", nameof(planId));

        return await _repository.GetByPlanIdAsync(planId);
    }

    public async Task<TrainingActionEntity> CreateAsync(TrainingActionEntity action)
    {
        ArgumentNullException.ThrowIfNull(action);
        ValidateAction(action);

        action.Id = Guid.NewGuid().ToString();
        action.CreatedAt = DateTime.UtcNow;
        action.UpdatedAt = DateTime.UtcNow;
        action.IsActive = true;

        _logger.LogInformation("Creation de l'action de formation '{Title}'.", action.Title);
        return await _repository.AddAsync(action);
    }

    public async Task<TrainingActionEntity> UpdateAsync(TrainingActionEntity action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var existing = await _repository.GetByIdAsync(action.Id)
            ?? throw new KeyNotFoundException($"L'action de formation '{action.Id}' est introuvable.");

        ValidateAction(action);

        action.UpdatedAt = DateTime.UtcNow;
        action.CreatedAt = existing.CreatedAt;
        action.CreatedBy = existing.CreatedBy;

        _logger.LogInformation("Mise a jour de l'action de formation {ActionId}.", action.Id);
        return await _repository.UpdateAsync(action);
    }

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant de l'action est obligatoire.", nameof(id));

        await _repository.DeleteAsync(id);
        _logger.LogInformation("Action de formation {ActionId} supprimee.", id);
    }

    private static void ValidateAction(TrainingActionEntity action)
    {
        if (string.IsNullOrWhiteSpace(action.Title))
            throw new ArgumentException("Le titre est requis.");

        if (action.DurationHours <= 0)
            throw new ArgumentException("La duree en heures doit etre superieure a 0.");

        if (action.Cost < 0)
            throw new ArgumentException("Le cout doit etre positif ou nul.");

        if (action.MaxParticipants < 0)
            throw new ArgumentException("Le nombre maximum de participants doit etre positif ou nul.");
    }
}
