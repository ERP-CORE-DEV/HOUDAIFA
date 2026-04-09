using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.Models.CertificationEcosysteme.Provider;
using Training.SkillDevelopment.Repositories.CertificationEcosysteme.Provider;

namespace Training.SkillDevelopment.Services.CertificationEcosysteme.Provider;

public sealed class TrainingProviderService : ITrainingProviderService
{
    private readonly ITrainingProviderRepository _repository;
    private readonly ILogger<TrainingProviderService> _logger;

    public TrainingProviderService(
        ITrainingProviderRepository repository,
        ILogger<TrainingProviderService> logger)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(logger);
        _repository = repository;
        _logger = logger;
    }

    public async Task<TrainingProvider?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant de l'organisme est obligatoire.", nameof(id));

        return await _repository.GetByIdAsync(id);
    }

    public Task<IReadOnlyList<TrainingProvider>> GetAllAsync()
        => _repository.GetAllAsync();

    public async Task<IReadOnlyList<TrainingProvider>> SearchByNameAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            throw new ArgumentException("Le terme de recherche est obligatoire.", nameof(searchTerm));

        return await _repository.SearchByNameAsync(searchTerm);
    }

    public async Task<bool> ValidateQualiopiAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant de l'organisme est obligatoire.", nameof(id));

        var provider = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"L'organisme de formation '{id}' est introuvable.");

        var isValid = provider.HasQualiopiCertification
                   && (provider.QualiopiExpirationDate is null
                       || provider.QualiopiExpirationDate.Value > DateTime.UtcNow);

        _logger.LogInformation(
            "Validation Qualiopi de l'organisme {ProviderId} '{Name}': {Result}.",
            id, provider.Name, isValid ? "certifie" : "non certifie");

        return isValid;
    }

    public async Task<decimal> GetProviderRatingAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant de l'organisme est obligatoire.", nameof(id));

        _ = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"L'organisme de formation '{id}' est introuvable.");

        _logger.LogInformation("Calcul de la note de l'organisme {ProviderId}.", id);

        return 0m;
    }

    public async Task<TrainingProvider> CreateAsync(TrainingProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ValidateProvider(provider);

        provider.Id = Guid.NewGuid().ToString();
        provider.IsActive = true;
        provider.CreatedAt = DateTime.UtcNow;
        provider.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Creation de l'organisme de formation '{Name}' (Qualiopi: {HasQualiopi}).",
            provider.Name, provider.HasQualiopiCertification);

        return await _repository.AddAsync(provider);
    }

    public async Task<TrainingProvider> UpdateAsync(TrainingProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        var existing = await _repository.GetByIdAsync(provider.Id)
            ?? throw new KeyNotFoundException($"L'organisme '{provider.Id}' est introuvable.");

        ValidateProvider(provider);

        provider.UpdatedAt = DateTime.UtcNow;
        provider.CreatedAt = existing.CreatedAt;
        provider.CreatedBy = existing.CreatedBy;

        _logger.LogInformation("Mise a jour de l'organisme {ProviderId}.", provider.Id);
        return await _repository.UpdateAsync(provider);
    }

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant de l'organisme est obligatoire.", nameof(id));

        await _repository.DeleteAsync(id);
        _logger.LogInformation("Organisme de formation {ProviderId} supprime.", id);
    }

    private static void ValidateProvider(TrainingProvider provider)
    {
        if (string.IsNullOrWhiteSpace(provider.Name))
            throw new ArgumentException("Le nom de l'organisme est requis.");

        if (string.IsNullOrWhiteSpace(provider.DeclarationNumber))
            throw new ArgumentException(
                "Le numero de declaration d'activite (NDA) est requis.");

        if (provider.HasQualiopiCertification
            && provider.QualiopiExpirationDate.HasValue
            && provider.QualiopiExpirationDate.Value < DateTime.UtcNow)
        {
            throw new ArgumentException(
                "La certification Qualiopi est expiree. Veuillez renouveler la certification.");
        }
    }
}
