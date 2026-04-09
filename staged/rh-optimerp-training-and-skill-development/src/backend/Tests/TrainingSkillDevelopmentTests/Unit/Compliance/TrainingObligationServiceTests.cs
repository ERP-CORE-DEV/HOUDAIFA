using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FinancementConformite.Compliance;
using Training.SkillDevelopment.Repositories.FinancementConformite.Compliance;
using Training.SkillDevelopment.Services.FinancementConformite.Compliance;
using Xunit;

namespace Training.SkillDevelopment.Tests.Unit.Compliance;

public sealed class TrainingObligationServiceTests
{
    private readonly Mock<ITrainingObligationRepository> _repositoryMock;
    private readonly Mock<ILogger<TrainingObligationService>> _loggerMock;
    private readonly TrainingObligationService _sut;

    public TrainingObligationServiceTests()
    {
        _repositoryMock = new Mock<ITrainingObligationRepository>();
        _loggerMock = new Mock<ILogger<TrainingObligationService>>();
        _sut = new TrainingObligationService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidObligation_SetsCurrentStatusAndIsActive()
    {
        // Arrange
        var obligation = BuildValidObligation();
        TrainingObligation? captured = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TrainingObligation>()))
            .Callback<TrainingObligation>(o => captured = o)
            .ReturnsAsync((TrainingObligation o) => o);

        // Act
        await _sut.CreateAsync(obligation);

        // Assert
        captured!.Status.Should().Be(ObligationStatus.Current);
        captured.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_EmptyTitle_ThrowsArgumentException()
    {
        // Arrange
        var obligation = BuildValidObligation();
        obligation.Title = string.Empty;

        // Act
        var act = async () => await _sut.CreateAsync(obligation);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*titre*");
    }

    [Fact]
    public async Task CreateAsync_ZeroFrequencyMonths_ThrowsArgumentException()
    {
        // Arrange
        var obligation = BuildValidObligation();
        obligation.FrequencyMonths = 0;

        // Act
        var act = async () => await _sut.CreateAsync(obligation);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*frequence*");
    }

    [Fact]
    public async Task CreateAsync_DefaultEffectiveDate_ThrowsArgumentException()
    {
        // Arrange
        var obligation = BuildValidObligation();
        obligation.EffectiveDate = default;

        // Act
        var act = async () => await _sut.CreateAsync(obligation);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*vigueur*");
    }

    [Fact]
    public async Task CreateAsync_ExpirationBeforeEffectiveDate_ThrowsArgumentException()
    {
        // Arrange
        var obligation = BuildValidObligation();
        obligation.EffectiveDate = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        obligation.ExpirationDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var act = async () => await _sut.CreateAsync(obligation);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*expiration*");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsObligation()
    {
        // Arrange
        var expected = BuildValidObligation();
        expected.Id = "obligation-001";
        _repositoryMock
            .Setup(r => r.GetByIdAsync("obligation-001"))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.GetByIdAsync("obligation-001");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("obligation-001");
    }

    [Fact]
    public async Task GetByIdAsync_EmptyId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.GetByIdAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*identifiant*");
    }

    [Fact]
    public async Task GetExpiringAsync_ZeroDaysAhead_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.GetExpiringAsync(0);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*jours*");
    }

    [Fact]
    public async Task GetExpiringAsync_ActiveObligationsExpiringSoon_ReturnsThem()
    {
        // Arrange
        var expiringIn30 = BuildValidObligation();
        expiringIn30.Id = "obligation-30days";
        expiringIn30.IsActive = true;
        expiringIn30.ExpirationDate = DateTime.UtcNow.AddDays(30);

        var expiringIn60 = BuildValidObligation();
        expiringIn60.Id = "obligation-60days";
        expiringIn60.IsActive = true;
        expiringIn60.ExpirationDate = DateTime.UtcNow.AddDays(60);

        var notExpiring = BuildValidObligation();
        notExpiring.Id = "obligation-ok";
        notExpiring.IsActive = true;
        notExpiring.ExpirationDate = DateTime.UtcNow.AddDays(200);

        _repositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<TrainingObligation> { expiringIn30, expiringIn60, notExpiring });

        // Act
        var result = await _sut.GetExpiringAsync(90);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRiskAssessmentAsync_ObligationExpiredYesterday_GeneratesCriticalAlert()
    {
        // Arrange
        var expired = BuildValidObligation();
        expired.Id = "obligation-expired";
        expired.Title = "Formation securite obligatoire";
        expired.IsActive = true;
        expired.ExpirationDate = DateTime.UtcNow.AddDays(-1);
        _repositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<TrainingObligation> { expired });

        // Act
        var result = await _sut.GetRiskAssessmentAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].RiskLevel.Should().Be(RiskLevel.Critical);
    }

    [Fact]
    public async Task GetRiskAssessmentAsync_ObligationExpiring25Days_GeneratesHighAlert()
    {
        // Arrange
        var expiringSoon = BuildValidObligation();
        expiringSoon.Id = "obligation-high";
        expiringSoon.IsActive = true;
        expiringSoon.ExpirationDate = DateTime.UtcNow.AddDays(25);
        _repositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<TrainingObligation> { expiringSoon });

        // Act
        var result = await _sut.GetRiskAssessmentAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].RiskLevel.Should().Be(RiskLevel.High);
    }

    [Fact]
    public async Task GetRiskAssessmentAsync_ObligationExpiring50Days_GeneratesMediumAlert()
    {
        // Arrange
        var expiringSoon = BuildValidObligation();
        expiringSoon.Id = "obligation-medium";
        expiringSoon.IsActive = true;
        expiringSoon.ExpirationDate = DateTime.UtcNow.AddDays(50);
        _repositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<TrainingObligation> { expiringSoon });

        // Act
        var result = await _sut.GetRiskAssessmentAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].RiskLevel.Should().Be(RiskLevel.Medium);
    }

    [Fact]
    public async Task GetRiskAssessmentAsync_ObligationExpiring80Days_GeneratesLowAlert()
    {
        // Arrange
        var expiringSoon = BuildValidObligation();
        expiringSoon.Id = "obligation-low";
        expiringSoon.IsActive = true;
        expiringSoon.ExpirationDate = DateTime.UtcNow.AddDays(80);
        _repositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<TrainingObligation> { expiringSoon });

        // Act
        var result = await _sut.GetRiskAssessmentAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].RiskLevel.Should().Be(RiskLevel.Low);
    }

    [Fact]
    public async Task GetRiskAssessmentAsync_ObligationExpiringOver90Days_GeneratesNoAlert()
    {
        // Arrange
        var notExpiring = BuildValidObligation();
        notExpiring.Id = "obligation-ok";
        notExpiring.IsActive = true;
        notExpiring.ExpirationDate = DateTime.UtcNow.AddDays(100);
        _repositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<TrainingObligation> { notExpiring });

        // Act
        var result = await _sut.GetRiskAssessmentAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_NonExistentObligation_ThrowsKeyNotFoundException()
    {
        // Arrange
        var obligation = BuildValidObligation();
        obligation.Id = "not-found";
        _repositoryMock
            .Setup(r => r.GetByIdAsync("not-found"))
            .ReturnsAsync((TrainingObligation?)null);

        // Act
        var act = async () => await _sut.UpdateAsync(obligation);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not-found*");
    }

    [Fact]
    public async Task DeleteAsync_ValidId_CallsRepositoryDelete()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.DeleteAsync("obligation-001"))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync("obligation-001");

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync("obligation-001"), Times.Once);
    }

    private static TrainingObligation BuildValidObligation() =>
        new()
        {
            Title = "Formation securite incendie",
            FrequencyMonths = 12,
            RegulatoryReference = "Article R4227-28 Code du travail",
            EffectiveDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ExpirationDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            RiskLevel = RiskLevel.High,
            IsActive = true
        };
}
