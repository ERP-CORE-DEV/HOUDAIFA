using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FinancementConformite.Compliance;
using Training.SkillDevelopment.Repositories.FinancementConformite.Compliance;

namespace Training.SkillDevelopment.Services.FinancementConformite.Compliance;

public sealed class TrainingObligationService : ITrainingObligationService
{
    private const int CriticalAlertDays = 30;
    private const int HighAlertDays = 60;
    private const int MediumAlertDays = 90;

    private readonly ITrainingObligationRepository _repository;
    private readonly ILogger<TrainingObligationService> _logger;

    public TrainingObligationService(
        ITrainingObligationRepository repository,
        ILogger<TrainingObligationService> logger)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(logger);
        _repository = repository;
        _logger = logger;
    }

    public async Task<TrainingObligation?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant de l'obligation est obligatoire.", nameof(id));

        return await _repository.GetByIdAsync(id);
    }

    public Task<IReadOnlyList<TrainingObligation>> GetAllAsync()
        => _repository.GetAllAsync();

    public async Task<IReadOnlyList<TrainingObligation>> GetExpiringAsync(int daysAhead = 90)
    {
        if (daysAhead <= 0)
            throw new ArgumentException("Le nombre de jours doit etre superieur a 0.", nameof(daysAhead));

        var all = await _repository.GetAllAsync();
        var threshold = DateTime.UtcNow.AddDays(daysAhead);

        var expiring = all
            .Where(o => o.IsActive
                     && o.ExpirationDate.HasValue
                     && o.ExpirationDate.Value <= threshold)
            .OrderBy(o => o.ExpirationDate)
            .ToList();

        _logger.LogInformation(
            "{Count} obligation(s) reglementaire(s) expirant dans les {Days} prochains jours.",
            expiring.Count, daysAhead);

        return expiring;
    }

    public async Task<IReadOnlyList<ComplianceAlert>> GetRiskAssessmentAsync()
    {
        var all = await _repository.GetAllAsync();
        var now = DateTime.UtcNow;
        var alerts = new List<ComplianceAlert>();

        foreach (var obligation in all.Where(o => o.IsActive && o.ExpirationDate.HasValue))
        {
            var daysUntilExpiry = (obligation.ExpirationDate!.Value - now).Days;

            if (daysUntilExpiry > MediumAlertDays)
                continue;

            var riskLevel = daysUntilExpiry switch
            {
                <= 0 => RiskLevel.Critical,
                <= CriticalAlertDays => RiskLevel.High,
                <= HighAlertDays => RiskLevel.Medium,
                _ => RiskLevel.Low
            };

            var message = daysUntilExpiry <= 0
                ? $"Obligation '{obligation.Title}' expiree depuis {Math.Abs(daysUntilExpiry)} jours."
                : $"Obligation '{obligation.Title}' expire dans {daysUntilExpiry} jours.";

            alerts.Add(new ComplianceAlert
            {
                Id = Guid.NewGuid().ToString(),
                ObligationId = obligation.Id,
                RiskLevel = riskLevel,
                Message = message,
                DueDate = obligation.ExpirationDate.Value,
                IsResolved = false,
                CreatedAt = now
            });
        }

        _logger.LogInformation(
            "Evaluation des risques de conformite: {Count} alerte(s) generee(s).", alerts.Count);

        return alerts;
    }

    public async Task<TrainingObligation> CreateAsync(TrainingObligation obligation)
    {
        ArgumentNullException.ThrowIfNull(obligation);
        ValidateObligation(obligation);

        obligation.Id = Guid.NewGuid().ToString();
        obligation.IsActive = true;
        obligation.Status = ObligationStatus.Current;
        obligation.CreatedAt = DateTime.UtcNow;
        obligation.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Creation de l'obligation reglementaire '{Title}' (ref: {Ref}).",
            obligation.Title, obligation.RegulatoryReference);

        return await _repository.AddAsync(obligation);
    }

    public async Task<TrainingObligation> UpdateAsync(TrainingObligation obligation)
    {
        ArgumentNullException.ThrowIfNull(obligation);

        var existing = await _repository.GetByIdAsync(obligation.Id)
            ?? throw new KeyNotFoundException($"L'obligation '{obligation.Id}' est introuvable.");

        ValidateObligation(obligation);

        obligation.UpdatedAt = DateTime.UtcNow;
        obligation.CreatedAt = existing.CreatedAt;
        obligation.CreatedBy = existing.CreatedBy;

        _logger.LogInformation("Mise a jour de l'obligation {ObligationId}.", obligation.Id);
        return await _repository.UpdateAsync(obligation);
    }

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant de l'obligation est obligatoire.", nameof(id));

        await _repository.DeleteAsync(id);
        _logger.LogInformation("Obligation reglementaire {ObligationId} supprimee.", id);
    }

    private static void ValidateObligation(TrainingObligation obligation)
    {
        if (string.IsNullOrWhiteSpace(obligation.Title))
            throw new ArgumentException("Le titre de l'obligation est requis.");

        if (obligation.FrequencyMonths <= 0)
            throw new ArgumentException("La frequence en mois doit etre superieure a 0.");

        if (obligation.EffectiveDate == default)
            throw new ArgumentException("La date d'entree en vigueur est requise.");

        if (obligation.ExpirationDate.HasValue && obligation.ExpirationDate.Value <= obligation.EffectiveDate)
            throw new ArgumentException(
                "La date d'expiration doit etre posterieure a la date d'entree en vigueur.");
    }
}
