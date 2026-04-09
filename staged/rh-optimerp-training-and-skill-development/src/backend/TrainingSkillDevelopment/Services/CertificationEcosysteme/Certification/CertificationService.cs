using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.Models.CertificationEcosysteme.Certification;
using Training.SkillDevelopment.Repositories.CertificationEcosysteme.Certification;

namespace Training.SkillDevelopment.Services.CertificationEcosysteme.Certification;

public sealed class CertificationService : ICertificationService
{
    private const string RncpPrefix = "RNCP";
    private const string RsPrefix = "RS";

    private readonly ICertificationRepository _repository;
    private readonly ILogger<CertificationService> _logger;

    public CertificationService(
        ICertificationRepository repository,
        ILogger<CertificationService> logger)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(logger);
        _repository = repository;
        _logger = logger;
    }

    public Task<CertificationRncp?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant de la certification est obligatoire.", nameof(id));

        return _repository.GetByIdAsync(id);
    }

    public Task<IReadOnlyList<CertificationRncp>> GetAllAsync()
        => _repository.GetAllAsync();

    public Task<CertificationRncp?> GetByRncpCodeAsync(string rncpCode)
    {
        if (string.IsNullOrWhiteSpace(rncpCode))
            throw new ArgumentException("Le code RNCP est obligatoire.", nameof(rncpCode));

        return _repository.GetByRncpCodeAsync(rncpCode);
    }

    public async Task<IReadOnlyList<CertificationRncp>> GetExpiringCertificationsAsync(int daysAhead = 90)
    {
        if (daysAhead <= 0)
            throw new ArgumentException("Le nombre de jours doit etre superieur a 0.", nameof(daysAhead));

        var all = await _repository.GetAllAsync();
        var threshold = DateTime.UtcNow.AddDays(daysAhead);

        var expiring = all
            .Where(c => c.IsActive && c.ExpirationDate.HasValue && c.ExpirationDate.Value <= threshold)
            .OrderBy(c => c.ExpirationDate)
            .ToList();

        _logger.LogInformation(
            "{Count} certification(s) expirant dans les {Days} prochains jours.",
            expiring.Count, daysAhead);

        return expiring;
    }

    public Task<bool> ValidateRncpCodeAsync(string rncpCode)
    {
        if (string.IsNullOrWhiteSpace(rncpCode))
            throw new ArgumentException("Le code RNCP est obligatoire.", nameof(rncpCode));

        var isValid = rncpCode.StartsWith(RncpPrefix, StringComparison.OrdinalIgnoreCase)
                   || rncpCode.StartsWith(RsPrefix, StringComparison.OrdinalIgnoreCase);

        _logger.LogInformation(
            "Validation du code RNCP '{Code}': {Result}.", rncpCode, isValid ? "valide" : "invalide");

        return Task.FromResult(isValid);
    }

    public async Task<CertificationRncp> CreateAsync(CertificationRncp certification)
    {
        ArgumentNullException.ThrowIfNull(certification);
        ValidateCertification(certification);

        var existing = await _repository.GetByRncpCodeAsync(certification.RncpCode);
        if (existing is not null)
            throw new InvalidOperationException(
                $"Une certification avec le code '{certification.RncpCode}' existe deja.");

        certification.Id = Guid.NewGuid().ToString();
        certification.IsActive = true;
        certification.CreatedAt = DateTime.UtcNow;
        certification.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Creation de la certification RNCP '{RncpCode}' - {Title}.",
            certification.RncpCode, certification.Title);

        return await _repository.AddAsync(certification);
    }

    public async Task<CertificationRncp> UpdateAsync(CertificationRncp certification)
    {
        ArgumentNullException.ThrowIfNull(certification);

        var existing = await _repository.GetByIdAsync(certification.Id)
            ?? throw new KeyNotFoundException($"La certification '{certification.Id}' est introuvable.");

        ValidateCertification(certification);

        certification.UpdatedAt = DateTime.UtcNow;
        certification.CreatedAt = existing.CreatedAt;
        certification.CreatedBy = existing.CreatedBy;

        _logger.LogInformation("Mise a jour de la certification {CertificationId}.", certification.Id);
        return await _repository.UpdateAsync(certification);
    }

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant de la certification est obligatoire.", nameof(id));

        await _repository.DeleteAsync(id);
        _logger.LogInformation("Certification {CertificationId} supprimee.", id);
    }

    private static void ValidateCertification(CertificationRncp certification)
    {
        if (string.IsNullOrWhiteSpace(certification.RncpCode))
            throw new ArgumentException("Le code RNCP est requis.");

        if (string.IsNullOrWhiteSpace(certification.Title))
            throw new ArgumentException("Le titre de la certification est requis.");

        if (certification.ExpirationDate.HasValue && certification.RegistrationDate.HasValue
            && certification.ExpirationDate.Value <= certification.RegistrationDate.Value)
        {
            throw new ArgumentException(
                "La date d'expiration doit etre posterieure a la date d'enregistrement.");
        }
    }
}
