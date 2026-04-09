using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.Models.FinancementConformite.Cpf;
using Training.SkillDevelopment.Repositories.FinancementConformite.Cpf;

namespace Training.SkillDevelopment.Services.FinancementConformite.Cpf;

public sealed class CpfService : ICpfService
{
    /// <summary>Abondement correctif suite au bilan sexennal non conforme (Art. L6315-1).</summary>
    private const decimal AbondementCorrectifEuros = 3000m;

    private readonly ICpfAccountRepository _repository;
    private readonly ILogger<CpfService> _logger;

    public CpfService(
        ICpfAccountRepository repository,
        ILogger<CpfService> logger)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(logger);
        _repository = repository;
        _logger = logger;
    }

    public Task<CpfAccount?> GetAccountByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("L'identifiant du compte CPF est obligatoire.", nameof(id));

        return _repository.GetByIdAsync(id, cancellationToken);
    }

    public Task<CpfAccount?> GetAccountByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(employeeId))
            throw new ArgumentException("L'identifiant de l'employe est obligatoire.", nameof(employeeId));

        return _repository.GetByEmployeeIdAsync(employeeId, cancellationToken);
    }

    public async Task<CpfAccount> CreateAccountAsync(CpfAccount account, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(account);

        if (string.IsNullOrWhiteSpace(account.EmployeeId))
            throw new ArgumentException("L'identifiant de l'employe est requis.");

        var existing = await _repository.GetByEmployeeIdAsync(account.EmployeeId, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException(
                $"Un compte CPF existe deja pour l'employe '{account.EmployeeId}'.");

        account.Id = Guid.NewGuid().ToString();
        account.AnnualCreditEuros = CalculateAnnualCredit(account.IsLowQualified);
        account.CeilingEuros = CalculateCeiling(account.IsLowQualified);
        account.BalanceEuros = 0;
        account.IsActive = true;
        account.CreatedAt = DateTime.UtcNow;
        account.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Creation du compte CPF pour l'employe {EmployeeId} (credit annuel: {Credit:C}, plafond: {Ceiling:C}).",
            account.EmployeeId, account.AnnualCreditEuros, account.CeilingEuros);

        return await _repository.CreateAsync(account, cancellationToken);
    }

    public async Task<CpfAccount> ApplyAnnualCreditAsync(string accountId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accountId))
            throw new ArgumentException("L'identifiant du compte CPF est obligatoire.", nameof(accountId));

        var account = await _repository.GetByIdAsync(accountId, cancellationToken)
            ?? throw new KeyNotFoundException($"Le compte CPF '{accountId}' est introuvable.");

        if (!account.IsActive)
            throw new InvalidOperationException("Le compte CPF est inactif.");

        var creditToApply = account.AnnualCreditEuros;
        var newBalance = account.BalanceEuros + creditToApply;

        if (newBalance > account.CeilingEuros)
        {
            creditToApply = account.CeilingEuros - account.BalanceEuros;
            newBalance = account.CeilingEuros;
            _logger.LogInformation(
                "Compte CPF {AccountId}: plafond atteint. Credit reduit a {Credit:C}.",
                accountId, creditToApply);
        }

        account.BalanceEuros = newBalance;
        account.LastCreditDate = DateTime.UtcNow;
        account.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Credit annuel CPF applique sur le compte {AccountId}: +{Credit:C}. Solde: {Balance:C}.",
            accountId, creditToApply, account.BalanceEuros);

        return await _repository.UpdateAsync(account, cancellationToken);
    }

    public async Task<CpfMobilization> MobilizeCpfAsync(
        CpfMobilization mobilization, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mobilization);

        if (string.IsNullOrWhiteSpace(mobilization.AccountId))
            throw new ArgumentException("L'identifiant du compte CPF est requis.");

        if (mobilization.Amount <= 0)
            throw new ArgumentException("Le montant de mobilisation doit etre superieur a 0.");

        var account = await _repository.GetByIdAsync(mobilization.AccountId, cancellationToken)
            ?? throw new KeyNotFoundException($"Le compte CPF '{mobilization.AccountId}' est introuvable.");

        mobilization.ResteACharge = ApplyResteACharge(mobilization.Amount);
        var effectiveAmount = mobilization.Amount - mobilization.ResteACharge;

        if (account.BalanceEuros < effectiveAmount)
            throw new InvalidOperationException(
                $"Solde CPF insuffisant. Disponible: {account.BalanceEuros:C}, demande: {effectiveAmount:C}.");

        account.BalanceEuros -= effectiveAmount;
        account.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(account, cancellationToken);

        mobilization.Id = Guid.NewGuid().ToString();
        mobilization.RequestDate = DateTime.UtcNow;
        mobilization.Status = "Approved";

        _logger.LogInformation(
            "Mobilisation CPF de {Amount:C} sur le compte {AccountId}. Reste a charge: {Rsc:C}. Nouveau solde: {Balance:C}.",
            mobilization.Amount, mobilization.AccountId, mobilization.ResteACharge, account.BalanceEuros);

        return mobilization;
    }

    public async Task<CpfAccount> ApplyAbondementCorrectifAsync(
        string accountId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accountId))
            throw new ArgumentException("L'identifiant du compte CPF est obligatoire.", nameof(accountId));

        var account = await _repository.GetByIdAsync(accountId, cancellationToken)
            ?? throw new KeyNotFoundException($"Le compte CPF '{accountId}' est introuvable.");

        account.BalanceEuros = Math.Min(account.BalanceEuros + AbondementCorrectifEuros, account.CeilingEuros);
        account.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Abondement correctif de {Amount:C} applique sur le compte CPF {AccountId} (Art. L6315-1). Nouveau solde: {Balance:C}.",
            AbondementCorrectifEuros, accountId, account.BalanceEuros);

        return await _repository.UpdateAsync(account, cancellationToken);
    }

    public decimal CalculateAnnualCredit(bool isLowQualified)
        => isLowQualified ? CpfAccount.LowQualifiedAnnualCredit : CpfAccount.StandardAnnualCredit;

    public decimal CalculateCeiling(bool isLowQualified)
        => isLowQualified ? CpfAccount.LowQualifiedCeiling : CpfAccount.StandardCeiling;

    public decimal ApplyResteACharge(decimal mobilizationAmount)
        => mobilizationAmount > 0 ? CpfAccount.ResteAChargeForfaitaire : 0m;
}
