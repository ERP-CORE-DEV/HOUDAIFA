using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FormationExecution.TrainingPlan;
using Training.SkillDevelopment.Repositories.FormationExecution.TrainingPlan;

namespace Training.SkillDevelopment.Services.FormationExecution.TrainingPlan;

public sealed class TrainingPlanService : ITrainingPlanService
{
    private const int LargeCompanyEmployeeThreshold = 50;
    private const decimal DefaultLegalObligationRate = 0.01m;

    private readonly ITrainingPlanRepository _repository;
    private readonly ILogger<TrainingPlanService> _logger;

    public TrainingPlanService(
        ITrainingPlanRepository repository,
        ILogger<TrainingPlanService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Models.FormationExecution.TrainingPlan.TrainingPlan?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant du plan est obligatoire.", nameof(id));

        return await _repository.GetByIdAsync(id);
    }

    public async Task<IReadOnlyList<Models.FormationExecution.TrainingPlan.TrainingPlan>> GetAllAsync()
        => await _repository.GetAllAsync();

    public async Task<IReadOnlyList<Models.FormationExecution.TrainingPlan.TrainingPlan>> GetByYearAsync(int year)
    {
        if (year < 2000 || year > 2100)
            throw new ArgumentException("L'annee doit etre comprise entre 2000 et 2100.", nameof(year));

        return await _repository.GetByYearAsync(year);
    }

    public async Task<Models.FormationExecution.TrainingPlan.TrainingPlan> CreateAsync(Models.FormationExecution.TrainingPlan.TrainingPlan plan)
    {
        ValidatePlan(plan);

        plan.Id = Guid.NewGuid().ToString();
        plan.Status = TrainingPlanStatus.Draft;
        plan.Version = 1;
        plan.CreatedAt = DateTime.UtcNow;
        plan.UpdatedAt = DateTime.UtcNow;
        plan.IsActive = true;

        if (plan.LegalObligationRate <= 0)
            plan.LegalObligationRate = DefaultLegalObligationRate;

        _logger.LogInformation("Creation du plan de formation '{Title}' pour l'annee {Year}.", plan.Title, plan.Year);
        return await _repository.AddAsync(plan);
    }

    public async Task<Models.FormationExecution.TrainingPlan.TrainingPlan> UpdateAsync(Models.FormationExecution.TrainingPlan.TrainingPlan plan)
    {
        var existing = await _repository.GetByIdAsync(plan.Id);
        if (existing is null)
            throw new KeyNotFoundException($"Le plan de formation '{plan.Id}' est introuvable.");

        ValidatePlan(plan);

        plan.UpdatedAt = DateTime.UtcNow;
        plan.Version = existing.Version + 1;
        plan.CreatedAt = existing.CreatedAt;
        plan.CreatedBy = existing.CreatedBy;

        _logger.LogInformation("Mise a jour du plan {PlanId} vers la version {Version}.", plan.Id, plan.Version);
        return await _repository.UpdateAsync(plan);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant du plan est obligatoire.", nameof(id));

        var deleted = await _repository.DeleteAsync(id);
        if (deleted)
            _logger.LogInformation("Plan de formation {PlanId} supprime.", id);

        return deleted;
    }

    public async Task<Models.FormationExecution.TrainingPlan.TrainingPlan> ApproveAsync(string id, string approvedBy)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant du plan est obligatoire.", nameof(id));

        if (string.IsNullOrWhiteSpace(approvedBy))
            throw new ArgumentException("L'identifiant de l'approbateur est obligatoire.", nameof(approvedBy));

        var plan = await _repository.GetByIdAsync(id);
        if (plan is null)
            throw new KeyNotFoundException($"Le plan de formation '{id}' est introuvable.");

        if (plan.Status != TrainingPlanStatus.PendingApproval)
            throw new InvalidOperationException(
                "Seuls les plans en attente d'approbation peuvent etre approuves.");

        plan.Status = TrainingPlanStatus.Approved;
        plan.ApprovedAt = DateTime.UtcNow;
        plan.ApprovedBy = approvedBy;
        plan.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation("Plan {PlanId} approuve par {ApprovedBy}.", id, approvedBy);
        return await _repository.UpdateAsync(plan);
    }

    public async Task<TrainingBudget> GetBudgetSummaryAsync(string planId)
    {
        if (string.IsNullOrWhiteSpace(planId))
            throw new ArgumentException("L'identifiant du plan est obligatoire.", nameof(planId));

        var plan = await _repository.GetByIdAsync(planId);
        if (plan is null)
            throw new KeyNotFoundException($"Le plan de formation '{planId}' est introuvable.");

        return new TrainingBudget
        {
            Id = Guid.NewGuid().ToString(),
            PlanId = planId,
            TotalBudget = plan.BudgetAllocated,
            ObligatoryBudget = plan.BudgetAllocated * 0.5m,
            DevelopmentBudget = plan.BudgetAllocated * 0.5m,
            MasseSalariale = plan.MasseSalariale,
            LegalObligationRate = plan.LegalObligationRate,
            Year = plan.Year,
            ConsumedAmount = plan.BudgetConsumed
        };
    }

    public async Task<PagedResult<Models.FormationExecution.TrainingPlan.TrainingPlan>> GetPagedAsync(int page, int pageSize)
    {
        if (page < 1)
            throw new ArgumentException("Le numero de page doit etre superieur a 0.", nameof(page));

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException("La taille de la page doit etre comprise entre 1 et 100.", nameof(pageSize));

        return await _repository.GetPagedAsync(page, pageSize);
    }

    private static void ValidatePlan(Models.FormationExecution.TrainingPlan.TrainingPlan plan)
    {
        if (string.IsNullOrWhiteSpace(plan.Title))
            throw new ArgumentException("Le titre est requis.");

        if (plan.Year < 2000 || plan.Year > 2100)
            throw new ArgumentException("L'annee est requise et doit etre valide.");

        if (plan.BudgetAllocated < 0)
            throw new ArgumentException("Le budget alloue doit etre positif ou nul.");

        if (plan.MasseSalariale > 0 && plan.BudgetAllocated < plan.MasseSalariale * plan.LegalObligationRate)
        {
            var minimumRequired = plan.MasseSalariale * plan.LegalObligationRate;
            throw new InvalidOperationException(
                $"Le budget alloue ({plan.BudgetAllocated:C}) est inferieur au minimum legal requis ({minimumRequired:C}, soit {plan.LegalObligationRate:P0} de la masse salariale).");
        }
    }
}
