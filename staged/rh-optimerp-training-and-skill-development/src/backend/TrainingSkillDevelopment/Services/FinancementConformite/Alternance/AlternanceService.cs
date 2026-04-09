using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.Models.FinancementConformite.Alternance;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Repositories.FinancementConformite.Alternance;

namespace Training.SkillDevelopment.Services.FinancementConformite.Alternance;

public sealed class AlternanceService : IAlternanceService
{
    /// <summary>SMIC mensuel brut 2024 (en euros).</summary>
    private const decimal SmicMensuelBrut = 1766.92m;

    private readonly IAlternanceContractRepository _repository;
    private readonly ILogger<AlternanceService> _logger;

    public AlternanceService(
        IAlternanceContractRepository repository,
        ILogger<AlternanceService> logger)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(logger);
        _repository = repository;
        _logger = logger;
    }

    public async Task<AlternanceContract?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant du contrat est obligatoire.", nameof(id));

        return await _repository.GetByIdAsync(id);
    }

    public async Task<IReadOnlyList<AlternanceContract>> GetByEmployeeIdAsync(string employeeId)
    {
        if (string.IsNullOrWhiteSpace(employeeId))
            throw new ArgumentException("L'identifiant de l'employe est obligatoire.", nameof(employeeId));

        return await _repository.GetByEmployeeIdAsync(employeeId);
    }

    public async Task<AlternanceContract> CreateAsync(AlternanceContract contract)
    {
        ArgumentNullException.ThrowIfNull(contract);
        ValidateContract(contract);

        contract.Id = Guid.NewGuid().ToString();
        contract.CreatedAt = DateTime.UtcNow;
        contract.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Creation d'un contrat d'alternance de type {Type} pour l'employe {EmployeeId}.",
            contract.Type, contract.EmployeeId);

        return await _repository.AddAsync(contract);
    }

    public async Task<AlternanceContract> UpdateAsync(AlternanceContract contract)
    {
        ArgumentNullException.ThrowIfNull(contract);

        var existing = await _repository.GetByIdAsync(contract.Id)
            ?? throw new KeyNotFoundException($"Le contrat d'alternance '{contract.Id}' est introuvable.");

        ValidateContract(contract);

        contract.UpdatedAt = DateTime.UtcNow;
        contract.CreatedAt = existing.CreatedAt;

        _logger.LogInformation("Mise a jour du contrat d'alternance {ContractId}.", contract.Id);
        return await _repository.UpdateAsync(contract);
    }

    public async Task<decimal> CalculateRemunerationAsync(string contractId)
    {
        if (string.IsNullOrWhiteSpace(contractId))
            throw new ArgumentException("L'identifiant du contrat est obligatoire.", nameof(contractId));

        var contract = await _repository.GetByIdAsync(contractId)
            ?? throw new KeyNotFoundException($"Le contrat d'alternance '{contractId}' est introuvable.");

        if (contract.RemunerationPercentage <= 0)
            throw new InvalidOperationException(
                "Le pourcentage de remuneration n'est pas defini sur ce contrat.");

        var remuneration = Math.Round(SmicMensuelBrut * (contract.RemunerationPercentage / 100m), 2);

        _logger.LogInformation(
            "Remuneration du contrat {ContractId}: {Percentage}% x SMIC ({Smic:C}) = {Remuneration:C}.",
            contractId, contract.RemunerationPercentage, SmicMensuelBrut, remuneration);

        return remuneration;
    }

    public Task<bool> CheckEligibilityAsync(string employeeId, DateTime birthDate)
    {
        if (string.IsNullOrWhiteSpace(employeeId))
            throw new ArgumentException("L'identifiant de l'employe est obligatoire.", nameof(employeeId));

        var ageAtToday = DateTime.UtcNow.Year - birthDate.Year;
        if (birthDate.Date > DateTime.UtcNow.AddYears(-ageAtToday))
            ageAtToday--;

        var isEligible = ageAtToday >= AlternanceContract.MinAge && ageAtToday <= AlternanceContract.MaxAge;

        _logger.LogInformation(
            "Verification d'eligibilite alternance pour l'employe {EmployeeId} (age: {Age}): {Eligible}.",
            employeeId, ageAtToday, isEligible ? "eligible" : "non eligible");

        return Task.FromResult(isEligible);
    }

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant du contrat est obligatoire.", nameof(id));

        await _repository.DeleteAsync(id);
        _logger.LogInformation("Contrat d'alternance {ContractId} supprime.", id);
    }

    private static void ValidateContract(AlternanceContract contract)
    {
        if (string.IsNullOrWhiteSpace(contract.EmployeeId))
            throw new ArgumentException("L'identifiant de l'alternant est requis.");

        if (contract.EndDate <= contract.StartDate)
            throw new ArgumentException("La date de fin doit etre posterieure a la date de debut.");

        if (contract.RemunerationPercentage <= 0 || contract.RemunerationPercentage > 100)
            throw new ArgumentException("Le pourcentage de remuneration doit etre compris entre 1 et 100.");
    }
}
