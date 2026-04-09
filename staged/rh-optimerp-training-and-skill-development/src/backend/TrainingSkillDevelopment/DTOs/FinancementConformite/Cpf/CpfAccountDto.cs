using System.ComponentModel.DataAnnotations;
using Training.SkillDevelopment.Models.FinancementConformite.Cpf;

namespace Training.SkillDevelopment.DTOs.FinancementConformite.Cpf;

public sealed class CpfAccountDto
{
    public string Id { get; init; } = string.Empty;

    [Required(ErrorMessage = "L'identifiant de l'employé est obligatoire.")]
    public string EmployeeId { get; init; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Le solde CPF ne peut pas être négatif.")]
    public decimal BalanceEuros { get; init; }

    public decimal CeilingEuros { get; init; }

    public decimal AnnualCreditEuros { get; init; }

    public bool IsLowQualified { get; init; }

    public DateTime? LastCreditDate { get; init; }

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }

    public bool IsAnonymized { get; init; }

    public static CpfAccountDto FromDomain(CpfAccount account)
    {
        ArgumentNullException.ThrowIfNull(account);

        return new CpfAccountDto
        {
            Id = account.Id,
            EmployeeId = account.EmployeeId,
            BalanceEuros = account.BalanceEuros,
            CeilingEuros = account.CeilingEuros,
            AnnualCreditEuros = account.AnnualCreditEuros,
            IsLowQualified = account.IsLowQualified,
            LastCreditDate = account.LastCreditDate,
            IsActive = account.IsActive,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt,
            IsAnonymized = account.IsAnonymized
        };
    }

    public CpfAccount ToDomain()
    {
        return new CpfAccount
        {
            Id = Id,
            EmployeeId = EmployeeId,
            BalanceEuros = BalanceEuros,
            CeilingEuros = CeilingEuros,
            AnnualCreditEuros = AnnualCreditEuros,
            IsLowQualified = IsLowQualified,
            LastCreditDate = LastCreditDate,
            IsActive = IsActive,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt,
            IsAnonymized = IsAnonymized
        };
    }
}
