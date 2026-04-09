using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.Models.CertificationEcosysteme.Certification;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Repositories.CertificationEcosysteme.Certification;

namespace Training.SkillDevelopment.Services.CertificationEcosysteme.Certification;

public sealed class VaeProjectService : IVaeProjectService
{
    private readonly IVaeProjectRepository _repository;
    private readonly ILogger<VaeProjectService> _logger;

    private static readonly VaeStatus[] PhaseOrder =
    [
        VaeStatus.Recevabilite,
        VaeStatus.Accompagnement,
        VaeStatus.Livret2,
        VaeStatus.Jury,
        VaeStatus.ValidationTotale
    ];

    public VaeProjectService(
        IVaeProjectRepository repository,
        ILogger<VaeProjectService> logger)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(logger);
        _repository = repository;
        _logger = logger;
    }

    public async Task<VaeProject?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant du projet VAE est obligatoire.", nameof(id));

        return await _repository.GetByIdAsync(id);
    }

    public async Task<IReadOnlyList<VaeProject>> GetByEmployeeIdAsync(string employeeId)
    {
        if (string.IsNullOrWhiteSpace(employeeId))
            throw new ArgumentException("L'identifiant de l'employe est obligatoire.", nameof(employeeId));

        return await _repository.GetByEmployeeIdAsync(employeeId);
    }

    public async Task<VaeProject> CreateAsync(VaeProject project)
    {
        ArgumentNullException.ThrowIfNull(project);
        ValidateProject(project);

        project.Id = Guid.NewGuid().ToString();
        project.Status = VaeStatus.Recevabilite;
        project.StartDate = DateTime.UtcNow;
        project.CreatedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Creation d'un projet VAE pour l'employe {EmployeeId} - RNCP: {RncpCode}.",
            project.EmployeeId, project.RncpCode);

        return await _repository.AddAsync(project);
    }

    public async Task<VaeProject> UpdateAsync(VaeProject project)
    {
        ArgumentNullException.ThrowIfNull(project);

        var existing = await _repository.GetByIdAsync(project.Id)
            ?? throw new KeyNotFoundException($"Le projet VAE '{project.Id}' est introuvable.");

        ValidateProject(project);

        project.UpdatedAt = DateTime.UtcNow;
        project.CreatedAt = existing.CreatedAt;
        project.CreatedBy = existing.CreatedBy;

        _logger.LogInformation("Mise a jour du projet VAE {ProjectId}.", project.Id);
        return await _repository.UpdateAsync(project);
    }

    public async Task<VaeProject> AdvancePhaseAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant du projet VAE est obligatoire.", nameof(id));

        var project = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Le projet VAE '{id}' est introuvable.");

        var currentIndex = Array.IndexOf(PhaseOrder, project.Status);
        if (currentIndex < 0 || currentIndex >= PhaseOrder.Length - 1)
        {
            throw new InvalidOperationException(
                $"Le projet VAE est en phase terminale '{project.Status}' et ne peut pas progresser.");
        }

        var nextPhase = PhaseOrder[currentIndex + 1];
        project.Status = nextPhase;
        project.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Projet VAE {ProjectId} avance vers la phase {Phase}.", id, nextPhase);

        return await _repository.UpdateAsync(project);
    }

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant du projet VAE est obligatoire.", nameof(id));

        await _repository.DeleteAsync(id);
        _logger.LogInformation("Projet VAE {ProjectId} supprime.", id);
    }

    private static void ValidateProject(VaeProject project)
    {
        if (string.IsNullOrWhiteSpace(project.EmployeeId))
            throw new ArgumentException("L'identifiant de l'employe est requis.");

        if (string.IsNullOrWhiteSpace(project.RncpCode) && string.IsNullOrWhiteSpace(project.CertificationId))
            throw new ArgumentException("Le code RNCP ou l'identifiant de la certification est requis.");
    }
}
