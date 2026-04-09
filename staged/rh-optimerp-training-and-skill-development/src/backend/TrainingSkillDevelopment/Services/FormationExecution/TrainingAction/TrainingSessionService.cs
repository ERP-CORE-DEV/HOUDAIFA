using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FormationExecution.TrainingAction;
using Training.SkillDevelopment.Repositories.FormationExecution.TrainingAction;

namespace Training.SkillDevelopment.Services.FormationExecution.TrainingAction;

public sealed class TrainingSessionService : ITrainingSessionService
{
    private readonly ITrainingSessionRepository _repository;
    private readonly ILogger<TrainingSessionService> _logger;

    public TrainingSessionService(
        ITrainingSessionRepository repository,
        ILogger<TrainingSessionService> logger)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(logger);
        _repository = repository;
        _logger = logger;
    }

    public async Task<TrainingSession?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant de la session est obligatoire.", nameof(id));

        return await _repository.GetByIdAsync(id);
    }

    public async Task<IReadOnlyList<TrainingSession>> GetByActionIdAsync(string actionId)
    {
        if (string.IsNullOrWhiteSpace(actionId))
            throw new ArgumentException("L'identifiant de l'action est obligatoire.", nameof(actionId));

        return await _repository.GetByActionIdAsync(actionId);
    }

    public async Task<IReadOnlyList<TrainingSession>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        if (end <= start)
            throw new ArgumentException("La date de fin doit etre posterieure a la date de debut.");

        return await _repository.GetByDateRangeAsync(start, end);
    }

    public async Task<TrainingSession> CreateAsync(TrainingSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        ValidateSession(session);

        session.Id = Guid.NewGuid().ToString();
        session.Status = SessionStatus.Planned;
        session.EnrolledCount = 0;
        session.CreatedAt = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;
        session.IsActive = true;

        _logger.LogInformation(
            "Creation d'une session pour l'action {ActionId} du {StartDate} au {EndDate}.",
            session.ActionId, session.StartDate, session.EndDate);

        return await _repository.AddAsync(session);
    }

    public async Task<TrainingSession> UpdateAsync(TrainingSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var existing = await _repository.GetByIdAsync(session.Id)
            ?? throw new KeyNotFoundException($"La session '{session.Id}' est introuvable.");

        ValidateSession(session);

        if (session.EnrolledCount > session.MaxCapacity)
            throw new InvalidOperationException(
                "Le nombre d'inscrits ne peut pas depasser la capacite maximale.");

        session.UpdatedAt = DateTime.UtcNow;
        session.CreatedAt = existing.CreatedAt;

        _logger.LogInformation("Mise a jour de la session {SessionId}.", session.Id);
        return await _repository.UpdateAsync(session);
    }

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant de la session est obligatoire.", nameof(id));

        await _repository.DeleteAsync(id);
        _logger.LogInformation("Session {SessionId} supprimee.", id);
    }

    private static void ValidateSession(TrainingSession session)
    {
        if (string.IsNullOrWhiteSpace(session.ActionId))
            throw new ArgumentException("L'identifiant de l'action est obligatoire.");

        if (session.EndDate <= session.StartDate)
            throw new ArgumentException("La date de fin doit etre posterieure a la date de debut.");

        if (session.MaxCapacity <= 0)
            throw new ArgumentException("La capacite maximale doit etre superieure a 0.");
    }
}
