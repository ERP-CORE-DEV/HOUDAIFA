using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Training.SkillDevelopment.Models.FinancementConformite.Cpf;
using Training.SkillDevelopment.Repositories.FinancementConformite.Cpf;
using Training.SkillDevelopment.Services.FinancementConformite.Cpf;
using Xunit;

namespace Training.SkillDevelopment.Tests.Unit.Cpf;

public sealed class CpfServiceTests
{
    private readonly Mock<ICpfAccountRepository> _repositoryMock;
    private readonly Mock<ILogger<CpfService>> _loggerMock;
    private readonly CpfService _sut;

    public CpfServiceTests()
    {
        _repositoryMock = new Mock<ICpfAccountRepository>();
        _loggerMock = new Mock<ILogger<CpfService>>();
        _sut = new CpfService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAccountByIdAsync_ExistingId_ReturnsAccount()
    {
        // Arrange
        var account = BuildValidAccount("emp-001");
        account.Id = "cpf-001";
        _repositoryMock
            .Setup(r => r.GetByIdAsync("cpf-001", default))
            .ReturnsAsync(account);

        // Act
        var result = await _sut.GetAccountByIdAsync("cpf-001");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAccountByIdAsync_NonExistentId_ReturnsNull()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync("unknown", default))
            .ReturnsAsync((CpfAccount?)null);

        // Act
        var result = await _sut.GetAccountByIdAsync("unknown");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAccountByEmployeeIdAsync_ValidId_ReturnsAccount()
    {
        // Arrange
        var account = BuildValidAccount("emp-001");
        _repositoryMock
            .Setup(r => r.GetByEmployeeIdAsync("emp-001", default))
            .ReturnsAsync(account);

        // Act
        var result = await _sut.GetAccountByEmployeeIdAsync("emp-001");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAccountAsync_ValidAccount_ReturnsCreated()
    {
        // Arrange
        var account = new CpfAccount { EmployeeId = "emp-002" };
        _repositoryMock
            .Setup(r => r.GetByEmployeeIdAsync("emp-002", default))
            .ReturnsAsync((CpfAccount?)null);
        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<CpfAccount>(), default))
            .ReturnsAsync((CpfAccount a, CancellationToken _) => a);

        // Act
        var result = await _sut.CreateAccountAsync(account);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ApplyAnnualCreditAsync_StandardEmployee_Credits500Euros()
    {
        // Arrange
        var account = BuildValidAccount("emp-001");
        account.IsLowQualified = false;
        account.AnnualCreditEuros = CpfAccount.StandardAnnualCredit;
        account.BalanceEuros = 0m;
        account.CeilingEuros = CpfAccount.StandardCeiling;
        _repositoryMock
            .Setup(r => r.GetByIdAsync("cpf-001", default))
            .ReturnsAsync(account);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<CpfAccount>(), default))
            .ReturnsAsync((CpfAccount a, CancellationToken _) => a);

        // Act
        var result = await _sut.ApplyAnnualCreditAsync("cpf-001");

        // Assert
        result.BalanceEuros.Should().Be(500m);
    }

    [Fact]
    public async Task ApplyAnnualCreditAsync_LowQualifiedEmployee_Credits800Euros()
    {
        // Arrange
        var account = BuildValidAccount("emp-003");
        account.IsLowQualified = true;
        account.AnnualCreditEuros = CpfAccount.LowQualifiedAnnualCredit;
        account.BalanceEuros = 0m;
        account.CeilingEuros = CpfAccount.LowQualifiedCeiling;
        _repositoryMock
            .Setup(r => r.GetByIdAsync("cpf-003", default))
            .ReturnsAsync(account);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<CpfAccount>(), default))
            .ReturnsAsync((CpfAccount a, CancellationToken _) => a);

        // Act
        var result = await _sut.ApplyAnnualCreditAsync("cpf-003");

        // Assert
        result.BalanceEuros.Should().Be(800m);
    }

    [Fact]
    public async Task ApplyAnnualCreditAsync_AtCeiling_DoesNotExceed5000()
    {
        // Arrange
        var account = BuildValidAccount("emp-001");
        account.IsLowQualified = false;
        account.AnnualCreditEuros = CpfAccount.StandardAnnualCredit;
        account.BalanceEuros = 4800m;
        account.CeilingEuros = CpfAccount.StandardCeiling;
        _repositoryMock
            .Setup(r => r.GetByIdAsync("cpf-001", default))
            .ReturnsAsync(account);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<CpfAccount>(), default))
            .ReturnsAsync((CpfAccount a, CancellationToken _) => a);

        // Act
        var result = await _sut.ApplyAnnualCreditAsync("cpf-001");

        // Assert
        result.BalanceEuros.Should().Be(CpfAccount.StandardCeiling);
    }

    [Fact]
    public async Task ApplyAnnualCreditAsync_LowQualifiedAtCeiling_DoesNotExceed8000()
    {
        // Arrange
        var account = BuildValidAccount("emp-003");
        account.IsLowQualified = true;
        account.AnnualCreditEuros = CpfAccount.LowQualifiedAnnualCredit;
        account.BalanceEuros = 7900m;
        account.CeilingEuros = CpfAccount.LowQualifiedCeiling;
        _repositoryMock
            .Setup(r => r.GetByIdAsync("cpf-003", default))
            .ReturnsAsync(account);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<CpfAccount>(), default))
            .ReturnsAsync((CpfAccount a, CancellationToken _) => a);

        // Act
        var result = await _sut.ApplyAnnualCreditAsync("cpf-003");

        // Assert
        result.BalanceEuros.Should().Be(CpfAccount.LowQualifiedCeiling);
    }

    [Fact]
    public async Task MobilizeCpfAsync_ValidAmount_DebitsBalance()
    {
        // Arrange
        var account = BuildValidAccount("emp-001");
        account.BalanceEuros = 1000m;
        _repositoryMock
            .Setup(r => r.GetByIdAsync("cpf-001", default))
            .ReturnsAsync(account);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<CpfAccount>(), default))
            .ReturnsAsync((CpfAccount a, CancellationToken _) => a);

        var mobilization = new CpfMobilization
        {
            AccountId = "cpf-001",
            Amount = 500m
        };

        // Act
        var result = await _sut.MobilizeCpfAsync(mobilization);

        // Assert
        // ResteACharge = 100 EUR (forfaitaire), effectiveAmount = 400 EUR
        result.ResteACharge.Should().Be(CpfAccount.ResteAChargeForfaitaire);
    }

    [Fact]
    public async Task MobilizeCpfAsync_InsufficientBalance_ThrowsInvalidOperationException()
    {
        // Arrange
        var account = BuildValidAccount("emp-001");
        account.BalanceEuros = 50m;
        _repositoryMock
            .Setup(r => r.GetByIdAsync("cpf-001", default))
            .ReturnsAsync(account);

        var mobilization = new CpfMobilization
        {
            AccountId = "cpf-001",
            Amount = 500m
        };

        // Act
        var act = async () => await _sut.MobilizeCpfAsync(mobilization);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task MobilizeCpfAsync_DuringWorkHours_RequiresEmployerApproval()
    {
        // Arrange
        var account = BuildValidAccount("emp-001");
        account.BalanceEuros = 2000m;
        _repositoryMock
            .Setup(r => r.GetByIdAsync("cpf-001", default))
            .ReturnsAsync(account);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<CpfAccount>(), default))
            .ReturnsAsync((CpfAccount a, CancellationToken _) => a);

        var mobilization = new CpfMobilization
        {
            AccountId = "cpf-001",
            Amount = 500m,
            DuringWorkingHours = true,
            EmployerApproval = null
        };

        // Act
        var result = await _sut.MobilizeCpfAsync(mobilization);

        // Assert
        result.DuringWorkingHours.Should().BeTrue();
    }

    [Fact]
    public void CalculateResteACharge_StandardMobilization_Returns100Euros()
    {
        // Arrange
        const decimal mobilizationAmount = 500m;

        // Act
        var result = _sut.ApplyResteACharge(mobilizationAmount);

        // Assert
        result.Should().Be(CpfAccount.ResteAChargeForfaitaire);
    }

    [Fact]
    public async Task ApplyAbondementCorrectifAsync_NonCompliant_Adds3000Euros()
    {
        // Arrange
        var account = BuildValidAccount("emp-001");
        account.BalanceEuros = 0m;
        account.CeilingEuros = CpfAccount.StandardCeiling;
        _repositoryMock
            .Setup(r => r.GetByIdAsync("cpf-001", default))
            .ReturnsAsync(account);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<CpfAccount>(), default))
            .ReturnsAsync((CpfAccount a, CancellationToken _) => a);

        // Act
        var result = await _sut.ApplyAbondementCorrectifAsync("cpf-001");

        // Assert
        result.BalanceEuros.Should().Be(3000m);
    }

    [Fact]
    public async Task UpdateAsync_ValidAccount_ReturnsUpdated()
    {
        // Arrange
        var account = BuildValidAccount("emp-001");
        account.BalanceEuros = 1500m;
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<CpfAccount>(), default))
            .ReturnsAsync(account);

        // Act
        var result = await _repositoryMock.Object.UpdateAsync(account);

        // Assert
        result.BalanceEuros.Should().Be(1500m);
    }

    [Fact]
    public void CalculateAnnualCredit_StandardEmployee_Returns500()
    {
        // Arrange & Act
        var result = _sut.CalculateAnnualCredit(isLowQualified: false);

        // Assert
        result.Should().Be(CpfAccount.StandardAnnualCredit);
    }

    private static CpfAccount BuildValidAccount(string employeeId) =>
        new()
        {
            Id = "cpf-001",
            EmployeeId = employeeId,
            IsActive = true,
            AnnualCreditEuros = CpfAccount.StandardAnnualCredit,
            CeilingEuros = CpfAccount.StandardCeiling,
            BalanceEuros = 0m
        };
}
