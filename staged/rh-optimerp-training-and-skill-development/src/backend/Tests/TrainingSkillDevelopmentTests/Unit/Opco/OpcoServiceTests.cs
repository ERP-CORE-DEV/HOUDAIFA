using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FinancementConformite.Opco;
using Training.SkillDevelopment.Repositories.FinancementConformite.Opco;
using Training.SkillDevelopment.Services.FinancementConformite.Opco;
using Xunit;
using OpcoModel = Training.SkillDevelopment.Models.FinancementConformite.Opco.Opco;

namespace Training.SkillDevelopment.Tests.Unit.Opco;

public sealed class OpcoServiceTests
{
    private readonly Mock<IOpcoRepository> _opcoRepositoryMock;
    private readonly Mock<IFundingApplicationRepository> _fundingRepositoryMock;
    private readonly Mock<ILogger<OpcoService>> _loggerMock;
    private readonly OpcoService _sut;

    public OpcoServiceTests()
    {
        _opcoRepositoryMock = new Mock<IOpcoRepository>();
        _fundingRepositoryMock = new Mock<IFundingApplicationRepository>();
        _loggerMock = new Mock<ILogger<OpcoService>>();
        _sut = new OpcoService(
            _opcoRepositoryMock.Object,
            _fundingRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidOpco_ReturnsCreatedOpco()
    {
        // Arrange
        var opco = BuildValidOpco();
        var created = BuildValidOpco();
        created.Id = Guid.NewGuid().ToString();
        _opcoRepositoryMock
            .Setup(r => r.GetByCodeAsync(opco.Code, default))
            .ReturnsAsync((OpcoModel?)null);
        _opcoRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<OpcoModel>(), default))
            .ReturnsAsync(created);

        // Act
        var result = await _sut.CreateAsync(opco);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateAsync_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var opco = BuildValidOpco();
        opco.Name = string.Empty;

        // Act
        var act = async () => await _sut.CreateAsync(opco);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*nom de l'OPCO*");
    }

    [Fact]
    public async Task CreateAsync_EmptyCode_ThrowsArgumentException()
    {
        // Arrange
        var opco = BuildValidOpco();
        opco.Code = string.Empty;

        // Act
        var act = async () => await _sut.CreateAsync(opco);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*code OPCO*");
    }

    [Fact]
    public async Task CreateAsync_DuplicateCode_ThrowsInvalidOperationException()
    {
        // Arrange
        var opco = BuildValidOpco();
        var existing = BuildValidOpco();
        existing.Id = "existing-001";
        _opcoRepositoryMock
            .Setup(r => r.GetByCodeAsync(opco.Code, default))
            .ReturnsAsync(existing);

        // Act
        var act = async () => await _sut.CreateAsync(opco);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{opco.Code}*");
    }

    [Fact]
    public async Task CreateAsync_NullOpco_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = async () => await _sut.CreateAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsOpco()
    {
        // Arrange
        var expected = BuildValidOpco();
        expected.Id = "opco-001";
        _opcoRepositoryMock
            .Setup(r => r.GetByIdAsync("opco-001", default))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.GetByIdAsync("opco-001");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("opco-001");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        // Arrange
        _opcoRepositoryMock
            .Setup(r => r.GetByIdAsync("unknown", default))
            .ReturnsAsync((OpcoModel?)null);

        // Act
        var result = await _sut.GetByIdAsync("unknown");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ExistingOpco_ReturnsUpdatedOpco()
    {
        // Arrange
        var existing = BuildValidOpco();
        existing.Id = "opco-001";
        var updated = BuildValidOpco();
        updated.Id = "opco-001";
        updated.Name = "OPCO Modifie";
        _opcoRepositoryMock
            .Setup(r => r.GetByIdAsync("opco-001", default))
            .ReturnsAsync(existing);
        _opcoRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<OpcoModel>(), default))
            .ReturnsAsync(updated);

        // Act
        var result = await _sut.UpdateAsync(updated);

        // Assert
        result.Name.Should().Be("OPCO Modifie");
    }

    [Fact]
    public async Task UpdateAsync_NonExistentOpco_ThrowsKeyNotFoundException()
    {
        // Arrange
        var opco = BuildValidOpco();
        opco.Id = "not-found";
        _opcoRepositoryMock
            .Setup(r => r.GetByIdAsync("not-found", default))
            .ReturnsAsync((OpcoModel?)null);

        // Act
        var act = async () => await _sut.UpdateAsync(opco);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not-found*");
    }

    [Fact]
    public async Task DeleteAsync_ExistingOpco_CallsRepositoryDelete()
    {
        // Arrange
        var existing = BuildValidOpco();
        existing.Id = "opco-001";
        _opcoRepositoryMock
            .Setup(r => r.GetByIdAsync("opco-001", default))
            .ReturnsAsync(existing);
        _opcoRepositoryMock
            .Setup(r => r.DeleteAsync("opco-001", default))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync("opco-001");

        // Assert
        _opcoRepositoryMock.Verify(r => r.DeleteAsync("opco-001", default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentOpco_ThrowsKeyNotFoundException()
    {
        // Arrange
        _opcoRepositoryMock
            .Setup(r => r.GetByIdAsync("not-found", default))
            .ReturnsAsync((OpcoModel?)null);

        // Act
        var act = async () => await _sut.DeleteAsync("not-found");

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CalculateContributionAsync_Under11Employees_AppliesLowerRate()
    {
        // Arrange
        const decimal masseSalariale = 100_000m;
        const int headcount = 5;

        // Act
        var result = await _sut.CalculateContributionAsync("company-001", 2025, masseSalariale, headcount);

        // Assert
        result.Should().NotBeNull();
        result.ContributionRate.Should().Be(TrainingContribution.RateUnder11);
        result.TotalAmount.Should().Be(Math.Round(masseSalariale * TrainingContribution.RateUnder11, 2));
    }

    [Fact]
    public async Task CalculateContributionAsync_AtLeast11Employees_AppliesHigherRate()
    {
        // Arrange
        const decimal masseSalariale = 500_000m;
        const int headcount = 25;

        // Act
        var result = await _sut.CalculateContributionAsync("company-001", 2025, masseSalariale, headcount);

        // Assert
        result.ContributionRate.Should().Be(TrainingContribution.Rate11Plus);
        result.TotalAmount.Should().Be(Math.Round(masseSalariale * TrainingContribution.Rate11Plus, 2));
    }

    [Fact]
    public async Task CalculateContributionAsync_ZeroMasseSalariale_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.CalculateContributionAsync("company-001", 2025, 0m, 10);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*masse salariale*");
    }

    [Fact]
    public async Task CalculateContributionAsync_ZeroHeadcount_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.CalculateContributionAsync("company-001", 2025, 100_000m, 0);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*effectif*");
    }

    [Fact]
    public async Task IsEligibleForFundingAsync_ActiveOpcoNoApprovedApplication_ReturnsTrue()
    {
        // Arrange
        var opco = BuildValidOpco();
        opco.Id = "opco-001";
        opco.IsActive = true;
        _opcoRepositoryMock
            .Setup(r => r.GetByIdAsync("opco-001", default))
            .ReturnsAsync(opco);
        _fundingRepositoryMock
            .Setup(r => r.GetByTrainingActionIdAsync("action-001", default))
            .ReturnsAsync(new List<FundingApplication>());

        // Act
        var result = await _sut.IsEligibleForFundingAsync("opco-001", "action-001");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsEligibleForFundingAsync_InactiveOpco_ReturnsFalse()
    {
        // Arrange
        var opco = BuildValidOpco();
        opco.Id = "opco-001";
        opco.IsActive = false;
        _opcoRepositoryMock
            .Setup(r => r.GetByIdAsync("opco-001", default))
            .ReturnsAsync(opco);

        // Act
        var result = await _sut.IsEligibleForFundingAsync("opco-001", "action-001");

        // Assert
        result.Should().BeFalse();
    }

    private static OpcoModel BuildValidOpco() =>
        new()
        {
            Name = "OPCO Sante",
            Code = "OPCO2I",
            IsActive = true
        };
}
