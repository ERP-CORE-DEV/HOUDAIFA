using System.ComponentModel.DataAnnotations;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FinancementConformite.Cpf;

namespace Training.SkillDevelopment.DTOs.FinancementConformite.Cpf;

public sealed class CpfTransactionDto
{
    public string Id { get; init; } = string.Empty;

    [Required(ErrorMessage = "L'identifiant du compte CPF est obligatoire.")]
    public string AccountId { get; init; } = string.Empty;

    public CpfTransactionType Type { get; init; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Le montant de la transaction doit être supérieur à zéro.")]
    public decimal Amount { get; init; }

    [Required(ErrorMessage = "La description de la transaction est obligatoire.")]
    public string Description { get; init; } = string.Empty;

    public string? TrainingActionId { get; init; }

    public DateTime TransactionDate { get; init; }

    [Required(ErrorMessage = "La source de la transaction est obligatoire.")]
    public string Source { get; init; } = string.Empty;

    public static CpfTransactionDto FromDomain(CpfTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        return new CpfTransactionDto
        {
            Id = transaction.Id,
            AccountId = transaction.AccountId,
            Type = transaction.Type,
            Amount = transaction.Amount,
            Description = transaction.Description,
            TrainingActionId = transaction.TrainingActionId,
            TransactionDate = transaction.TransactionDate,
            Source = transaction.Source
        };
    }

    public CpfTransaction ToDomain()
    {
        return new CpfTransaction
        {
            Id = Id,
            AccountId = AccountId,
            Type = Type,
            Amount = Amount,
            Description = Description,
            TrainingActionId = TrainingActionId,
            TransactionDate = TransactionDate,
            Source = Source
        };
    }
}
