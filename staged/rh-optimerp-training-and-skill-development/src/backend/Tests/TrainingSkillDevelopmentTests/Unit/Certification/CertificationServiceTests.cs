using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Training.SkillDevelopment.Models.CertificationEcosysteme.Certification;
using Training.SkillDevelopment.Repositories.CertificationEcosysteme.Certification;
using Training.SkillDevelopment.Services.CertificationEcosysteme.Certification;
using Xunit;

namespace Training.SkillDevelopment.Tests.Unit.Certification;

public sealed class CertificationServiceTests
{
    private readonly Mock<ICertificationRepository> _repositoryMock;
    private readonly Mock<ILogger<CertificationService>> _loggerMock;
    private readonly CertificationService _sut;

    public CertificationServiceTests()
    {
        _repositoryMock = new Mock<ICertificationRepository>();
        _loggerMock = new Mock<ILogger<CertificationService>>();
        _sut = new CertificationService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidCertification_ReturnsCreatedCertification()
    {
        // Arrange
        var certification = BuildValidCertification();
        var created = BuildValidCertification();
        created.Id = Guid.NewGuid().ToString();
        _repositoryMock
            .Setup(r => r.GetByRncpCodeAsync(certification.RncpCode))
            .ReturnsAsync((CertificationRncp?)null);
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<CertificationRncp>()))
            .ReturnsAsync(created);

        // Act
        var result = await _sut.CreateAsync(certification);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateAsync_EmptyRncpCode_ThrowsArgumentException()
    {
        // Arrange
        var certification = BuildValidCertification();
        certification.RncpCode = string.Empty;

        // Act
        var act = async () => await _sut.CreateAsync(certification);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*RNCP*");
    }

    [Fact]
    public async Task CreateAsync_EmptyTitle_ThrowsArgumentException()
    {
        // Arrange
        var certification = BuildValidCertification();
        certification.Title = string.Empty;

        // Act
        var act = async () => await _sut.CreateAsync(certification);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*titre*");
    }

    [Fact]
    public async Task CreateAsync_DuplicateRncpCode_ThrowsInvalidOperationException()
    {
        // Arrange
        var certification = BuildValidCertification();
        var existing = BuildValidCertification();
        existing.Id = "cert-existing";
        _repositoryMock
            .Setup(r => r.GetByRncpCodeAsync(certification.RncpCode))
            .ReturnsAsync(existing);

        // Act
        var act = async () => await _sut.CreateAsync(certification);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{certification.RncpCode}*");
    }

    [Fact]
    public async Task CreateAsync_ExpirationBeforeRegistration_ThrowsArgumentException()
    {
        // Arrange
        var certification = BuildValidCertification();
        certification.RegistrationDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        certification.ExpirationDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _repositoryMock
            .Setup(r => r.GetByRncpCodeAsync(certification.RncpCode))
            .ReturnsAsync((CertificationRncp?)null);

        // Act
        var act = async () => await _sut.CreateAsync(certification);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*expiration*");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCertification()
    {
        // Arrange
        var expected = BuildValidCertification();
        expected.Id = "cert-001";
        _repositoryMock
            .Setup(r => r.GetByIdAsync("cert-001"))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.GetByIdAsync("cert-001");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("cert-001");
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
    public async Task ValidateRncpCodeAsync_ValidRncpPrefix_ReturnsTrue()
    {
        // Arrange & Act
        var result = await _sut.ValidateRncpCodeAsync("RNCP38654");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateRncpCodeAsync_ValidRsPrefix_ReturnsTrue()
    {
        // Arrange & Act
        var result = await _sut.ValidateRncpCodeAsync("RS6489");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateRncpCodeAsync_InvalidPrefix_ReturnsFalse()
    {
        // Arrange & Act
        var result = await _sut.ValidateRncpCodeAsync("INVALID123");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateRncpCodeAsync_EmptyCode_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.ValidateRncpCodeAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*RNCP*");
    }

    [Fact]
    public async Task GetExpiringCertificationsAsync_NegativeDaysAhead_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.GetExpiringCertificationsAsync(-1);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*jours*");
    }

    [Fact]
    public async Task GetExpiringCertificationsAsync_ActiveCertificationsExpiringSoon_ReturnsThem()
    {
        // Arrange
        var expiringSoon = BuildValidCertification();
        expiringSoon.Id = "cert-expiring";
        expiringSoon.IsActive = true;
        expiringSoon.ExpirationDate = DateTime.UtcNow.AddDays(30);

        var notExpiring = BuildValidCertification();
        notExpiring.Id = "cert-ok";
        notExpiring.IsActive = true;
        notExpiring.ExpirationDate = DateTime.UtcNow.AddDays(200);

        _repositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<CertificationRncp> { expiringSoon, notExpiring });

        // Act
        var result = await _sut.GetExpiringCertificationsAsync(90);

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("cert-expiring");
    }

    [Fact]
    public async Task UpdateAsync_NonExistentCertification_ThrowsKeyNotFoundException()
    {
        // Arrange
        var certification = BuildValidCertification();
        certification.Id = "not-found";
        _repositoryMock
            .Setup(r => r.GetByIdAsync("not-found"))
            .ReturnsAsync((CertificationRncp?)null);

        // Act
        var act = async () => await _sut.UpdateAsync(certification);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not-found*");
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_CallsRepositoryDelete()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.DeleteAsync("cert-001"))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync("cert-001");

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync("cert-001"), Times.Once);
    }

    private static CertificationRncp BuildValidCertification() =>
        new()
        {
            RncpCode = "RNCP38654",
            Title = "Developpeur Web et Web Mobile",
            IsActive = true
        };
}
