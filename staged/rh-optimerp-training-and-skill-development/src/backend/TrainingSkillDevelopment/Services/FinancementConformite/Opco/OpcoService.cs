using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FinancementConformite.Opco;
using Training.SkillDevelopment.Repositories.FinancementConformite.Opco;

namespace Training.SkillDevelopment.Services.FinancementConformite.Opco;

public sealed class OpcoService : IOpcoService
{
    private readonly IOpcoRepository _opcoRepository;
    private readonly IFundingApplicationRepository _fundingApplicationRepository;
    private readonly ILogger<OpcoService> _logger;

    public OpcoService(
        IOpcoRepository opcoRepository,
        IFundingApplicationRepository fundingApplicationRepository,
        ILogger<OpcoService> logger)
    {
        _opcoRepository = opcoRepository ?? throw new ArgumentNullException(nameof(opcoRepository));
        _fundingApplicationRepository = fundingApplicationRepository ?? throw new ArgumentNullException(nameof(fundingApplicationRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Models.FinancementConformite.Opco.Opco?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => _opcoRepository.GetByIdAsync(id, cancellationToken);

    public Task<Models.FinancementConformite.Opco.Opco?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => _opcoRepository.GetByCodeAsync(code, cancellationToken);

    public Task<PagedResult<Models.FinancementConformite.Opco.Opco>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => _opcoRepository.GetAllAsync(page, pageSize, cancellationToken);

    public Task<IReadOnlyList<Models.FinancementConformite.Opco.Opco>> GetActiveAsync(CancellationToken cancellationToken = default)
        => _opcoRepository.GetActiveAsync(cancellationToken);

    public async Task<Models.FinancementConformite.Opco.Opco> CreateAsync(Models.FinancementConformite.Opco.Opco opco, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(opco);

        if (string.IsNullOrWhiteSpace(opco.Name))
            throw new ArgumentException("Le nom de l'OPCO est obligatoire.", nameof(opco));

        if (string.IsNullOrWhiteSpace(opco.Code))
            throw new ArgumentException("Le code OPCO est obligatoire.", nameof(opco));

        var existing = await _opcoRepository.GetByCodeAsync(opco.Code, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException($"Un OPCO avec le code '{opco.Code}' existe deja.");

        opco.Id = Guid.NewGuid().ToString();

        _logger.LogInformation("Creation de l'OPCO {OpcoCode} - {OpcoName}", opco.Code, opco.Name);
        return await _opcoRepository.CreateAsync(opco, cancellationToken);
    }

    public async Task<Models.FinancementConformite.Opco.Opco> UpdateAsync(Models.FinancementConformite.Opco.Opco opco, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(opco);

        var existing = await _opcoRepository.GetByIdAsync(opco.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"OPCO introuvable : {opco.Id}");

        _logger.LogInformation("Mise a jour de l'OPCO {OpcoId}", opco.Id);
        return await _opcoRepository.UpdateAsync(opco, cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await _opcoRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"OPCO introuvable : {id}");

        _logger.LogInformation("Suppression de l'OPCO {OpcoId}", id);
        await _opcoRepository.DeleteAsync(id, cancellationToken);
    }

    public Task<TrainingContribution> CalculateContributionAsync(
        string companyId,
        int year,
        decimal masseSalariale,
        int headcount,
        CancellationToken cancellationToken = default)
    {
        if (masseSalariale <= 0)
            throw new ArgumentException("La masse salariale doit etre positive.", nameof(masseSalariale));

        if (headcount <= 0)
            throw new ArgumentException("L'effectif doit etre positif.", nameof(headcount));

        var rate = headcount < 11 ? TrainingContribution.RateUnder11 : TrainingContribution.Rate11Plus;
        var totalAmount = Math.Round(masseSalariale * rate, 2);

        var contribution = new TrainingContribution
        {
            Id = Guid.NewGuid().ToString(),
            CompanyId = companyId,
            Year = year,
            MasseSalariale = masseSalariale,
            ContributionRate = rate,
            TotalAmount = totalAmount
        };

        _logger.LogInformation(
            "Calcul contribution formation entreprise {CompanyId} annee {Year}: {Rate:P2} x {MasseSalariale:C} = {Total:C}",
            companyId, year, rate, masseSalariale, totalAmount);

        return Task.FromResult(contribution);
    }

    public async Task<bool> IsEligibleForFundingAsync(string opcoId, string trainingActionId, CancellationToken cancellationToken = default)
    {
        var opco = await _opcoRepository.GetByIdAsync(opcoId, cancellationToken);
        if (opco is null || !opco.IsActive)
        {
            _logger.LogWarning("OPCO {OpcoId} inactif ou introuvable pour le financement", opcoId);
            return false;
        }

        var existingApplications = await _fundingApplicationRepository
            .GetByTrainingActionIdAsync(trainingActionId, cancellationToken);

        var hasApprovedApplication = existingApplications.Any(a =>
            a.OpcoId == opcoId && a.Status == FundingStatus.Approved);

        if (hasApprovedApplication)
        {
            _logger.LogInformation(
                "Une demande de financement approuvee existe deja pour l'action {TrainingActionId} aupres de l'OPCO {OpcoId}",
                trainingActionId, opcoId);
            return false;
        }

        return true;
    }
}
