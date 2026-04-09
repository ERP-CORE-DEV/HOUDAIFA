using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FinancementConformite.Opco;
using Training.SkillDevelopment.Repositories.FinancementConformite.Opco;

namespace Training.SkillDevelopment.Services.FinancementConformite.Opco;

public sealed class FundingApplicationService : IFundingApplicationService
{
    private readonly IFundingApplicationRepository _repository;
    private readonly ILogger<FundingApplicationService> _logger;

    public FundingApplicationService(
        IFundingApplicationRepository repository,
        ILogger<FundingApplicationService> logger)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(logger);
        _repository = repository;
        _logger = logger;
    }

    public Task<FundingApplication?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant de la demande est obligatoire.", nameof(id));

        return _repository.GetByIdAsync(id, cancellationToken);
    }

    public Task<PagedResult<FundingApplication>> GetByOpcoIdAsync(
        string opcoId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(opcoId))
            throw new ArgumentException("L'identifiant de l'OPCO est obligatoire.", nameof(opcoId));

        ValidatePagination(page, pageSize);
        return _repository.GetByOpcoIdAsync(opcoId, page, pageSize, cancellationToken);
    }

    public Task<PagedResult<FundingApplication>> GetByStatusAsync(
        FundingStatus status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        ValidatePagination(page, pageSize);
        return _repository.GetByStatusAsync(status, page, pageSize, cancellationToken);
    }

    public async Task<FundingApplication> SubmitAsync(
        FundingApplication application, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(application);
        ValidateApplication(application);

        application.Id = Guid.NewGuid().ToString();
        application.Status = FundingStatus.Submitted;
        application.SubmissionDate = DateTime.UtcNow;
        application.CreatedAt = DateTime.UtcNow;
        application.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Soumission d'une demande de financement OPCO {OpcoId} pour l'action {ActionId} - montant: {Amount:C}.",
            application.OpcoId, application.TrainingActionId, application.RequestedAmount);

        return await _repository.CreateAsync(application, cancellationToken);
    }

    public async Task<FundingApplication> ApproveAsync(
        string id, decimal grantedAmount, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant de la demande est obligatoire.", nameof(id));

        if (grantedAmount <= 0)
            throw new ArgumentException("Le montant accorde doit etre superieur a 0.", nameof(grantedAmount));

        var application = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"La demande de financement '{id}' est introuvable.");

        if (application.Status != FundingStatus.UnderReview && application.Status != FundingStatus.Submitted)
            throw new InvalidOperationException(
                "Seules les demandes soumises ou en cours d'examen peuvent etre approuvees.");

        application.Status = FundingStatus.Approved;
        application.GrantedAmount = grantedAmount;
        application.DecisionDate = DateTime.UtcNow;
        application.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Demande de financement {ApplicationId} approuvee pour {Amount:C}.", id, grantedAmount);

        return await _repository.UpdateAsync(application, cancellationToken);
    }

    public async Task<FundingApplication> RejectAsync(
        string id, string rejectionReason, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant de la demande est obligatoire.", nameof(id));

        if (string.IsNullOrWhiteSpace(rejectionReason))
            throw new ArgumentException("Le motif de rejet est obligatoire.", nameof(rejectionReason));

        var application = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"La demande de financement '{id}' est introuvable.");

        if (application.Status == FundingStatus.Approved || application.Status == FundingStatus.Paid)
            throw new InvalidOperationException(
                "Une demande approuvee ou payee ne peut pas etre rejetee.");

        application.Status = FundingStatus.Rejected;
        application.RejectionReason = rejectionReason;
        application.DecisionDate = DateTime.UtcNow;
        application.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Demande de financement {ApplicationId} rejetee. Motif: {Reason}.", id, rejectionReason);

        return await _repository.UpdateAsync(application, cancellationToken);
    }

    public async Task<FundingApplication> UpdateAsync(
        FundingApplication application, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(application);

        _ = await _repository.GetByIdAsync(application.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"La demande de financement '{application.Id}' est introuvable.");

        ValidateApplication(application);

        application.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation("Mise a jour de la demande de financement {ApplicationId}.", application.Id);
        return await _repository.UpdateAsync(application, cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant de la demande est obligatoire.", nameof(id));

        await _repository.DeleteAsync(id, cancellationToken);
        _logger.LogInformation("Demande de financement {ApplicationId} supprimee.", id);
    }

    private static void ValidateApplication(FundingApplication application)
    {
        if (string.IsNullOrWhiteSpace(application.OpcoId))
            throw new ArgumentException("L'identifiant de l'OPCO est requis.");

        if (string.IsNullOrWhiteSpace(application.TrainingActionId))
            throw new ArgumentException("L'identifiant de l'action de formation est requis.");

        if (application.RequestedAmount <= 0)
            throw new ArgumentException("Le montant demande doit etre superieur a 0.");

        if (application.EmployeeIds.Length == 0)
            throw new ArgumentException("Au moins un employe doit etre associe a la demande.");
    }

    private static void ValidatePagination(int page, int pageSize)
    {
        if (page < 1)
            throw new ArgumentException("Le numero de page doit etre superieur a 0.", nameof(page));

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException("La taille de la page doit etre comprise entre 1 et 100.", nameof(pageSize));
    }
}
