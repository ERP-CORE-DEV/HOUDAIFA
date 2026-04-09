using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Training.SkillDevelopment.Models.FinancementConformite.Alternance;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Repositories.FinancementConformite.Alternance;
using Training.SkillDevelopment.Services.FinancementConformite.Alternance;
using Xunit;

namespace Training.SkillDevelopment.Tests.Unit.Alternance;

public sealed class AlternanceServiceTests
{
    private const decimal SmicMensuelBrut = 1766.92m;

    private readonly Mock<IAlternanceContractRepository> _repositoryMock;
    private readonly Mock<ILogger<AlternanceService>> _loggerMock;
    private readonly AlternanceService _sut;

    public AlternanceServiceTests()
    {
        _repositoryMock = new Mock<IAlternanceContractRepository>();
        _loggerMock = new Mock<ILogger<AlternanceService>>();
        _sut = new AlternanceService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidContract_ReturnsCreatedContract()
    {
        // Arrange
        var contract = BuildValidContract();
        var created = BuildValidContract();
        created.Id = Guid.NewGuid().ToString();
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<AlternanceContract>()))
            .ReturnsAsync(created);

        // Act
        var result = await _sut.CreateAsync(contract);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateAsync_MissingEmployeeId_ThrowsArgumentException()
    {
        // Arrange
        var contract = BuildValidContract();
        contract.EmployeeId = string.Empty;

        // Act
        var act = async () => await _sut.CreateAsync(contract);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*alternant*");
    }

    [Fact]
    public async Task CreateAsync_EndDateBeforeStartDate_ThrowsArgumentException()
    {
        // Arrange
        var contract = BuildValidContract();
        contract.StartDate = new DateTime(2025, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        contract.EndDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var act = async () => await _sut.CreateAsync(contract);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*fin*");
    }

    [Fact]
    public async Task CreateAsync_ZeroRemunerationPercentage_ThrowsArgumentException()
    {
        // Arrange
        var contract = BuildValidContract();
        contract.RemunerationPercentage = 0m;

        // Act
        var act = async () => await _sut.CreateAsync(contract);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*remuneration*");
    }

    [Fact]
    public async Task CreateAsync_RemunerationOver100Percent_ThrowsArgumentException()
    {
        // Arrange
        var contract = BuildValidContract();
        contract.RemunerationPercentage = 101m;

        // Act
        var act = async () => await _sut.CreateAsync(contract);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*remuneration*");
    }

    [Fact]
    public async Task CalculateRemunerationAsync_ValidContract_ReturnsCorrectAmount()
    {
        // Arrange
        var contract = BuildValidContract();
        contract.Id = "contract-001";
        contract.RemunerationPercentage = 43m;
        _repositoryMock
            .Setup(r => r.GetByIdAsync("contract-001"))
            .ReturnsAsync(contract);

        // Act
        var result = await _sut.CalculateRemunerationAsync("contract-001");

        // Assert
        var expected = Math.Round(SmicMensuelBrut * (43m / 100m), 2);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task CalculateRemunerationAsync_ZeroPercentage_ThrowsInvalidOperationException()
    {
        // Arrange
        var contract = BuildValidContract();
        contract.Id = "contract-001";
        contract.RemunerationPercentage = 0m;
        _repositoryMock
            .Setup(r => r.GetByIdAsync("contract-001"))
            .ReturnsAsync(contract);

        // Act
        var act = async () => await _sut.CalculateRemunerationAsync("contract-001");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*pourcentage*");
    }

    [Fact]
    public async Task CalculateRemunerationAsync_NonExistentContract_ThrowsKeyNotFoundException()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync("not-found"))
            .ReturnsAsync((AlternanceContract?)null);

        // Act
        var act = async () => await _sut.CalculateRemunerationAsync("not-found");

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CheckEligibilityAsync_AgeWithinRange_ReturnsTrue()
    {
        // Arrange
        // A person currently 22 years old
        var birthDate = DateTime.UtcNow.AddYears(-22);

        // Act
        var result = await _sut.CheckEligibilityAsync("emp-001", birthDate);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckEligibilityAsync_AgeBelowMinimum_ReturnsFalse()
    {
        // Arrange
        // A person currently 15 years old (below MinAge = 16)
        var birthDate = DateTime.UtcNow.AddYears(-15);

        // Act
        var result = await _sut.CheckEligibilityAsync("emp-001", birthDate);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckEligibilityAsync_AgeAboveMaximum_ReturnsFalse()
    {
        // Arrange
        // A person currently 30 years old (above MaxAge = 29)
        var birthDate = DateTime.UtcNow.AddYears(-30);

        // Act
        var result = await _sut.CheckEligibilityAsync("emp-001", birthDate);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckEligibilityAsync_AgeAtMinimumBoundary_ReturnsTrue()
    {
        // Arrange
        // A person exactly 16 years old
        var birthDate = DateTime.UtcNow.AddYears(-16);

        // Act
        var result = await _sut.CheckEligibilityAsync("emp-001", birthDate);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckEligibilityAsync_EmptyEmployeeId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.CheckEligibilityAsync(string.Empty, DateTime.UtcNow.AddYears(-22));

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*employe*");
    }

    [Fact]
    public void AlternanceContract_TrialPeriodDays_Is45()
    {
        // Arrange & Act & Assert
        AlternanceContract.TrialPeriodDays.Should().Be(45);
    }

    [Fact]
    public async Task GetByEmployeeIdAsync_EmptyEmployeeId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.GetByEmployeeIdAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*employe*");
    }

    [Fact]
    public async Task UpdateAsync_NonExistentContract_ThrowsKeyNotFoundException()
    {
        // Arrange
        var contract = BuildValidContract();
        contract.Id = "not-found";
        _repositoryMock
            .Setup(r => r.GetByIdAsync("not-found"))
            .ReturnsAsync((AlternanceContract?)null);

        // Act
        var act = async () => await _sut.UpdateAsync(contract);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    private static AlternanceContract BuildValidContract() =>
        new()
        {
            EmployeeId = "emp-001",
            Type = AlternanceContractType.Apprentissage,
            StartDate = new DateTime(2025, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2027, 8, 31, 0, 0, 0, DateTimeKind.Utc),
            RemunerationPercentage = 43m,
            Status = "Active"
        };
}
