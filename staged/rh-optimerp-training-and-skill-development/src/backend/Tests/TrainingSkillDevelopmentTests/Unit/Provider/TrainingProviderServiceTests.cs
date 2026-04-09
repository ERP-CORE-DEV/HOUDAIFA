using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Training.SkillDevelopment.Models.CertificationEcosysteme.Provider;
using Training.SkillDevelopment.Repositories.CertificationEcosysteme.Provider;
using Training.SkillDevelopment.Services.CertificationEcosysteme.Provider;
using Xunit;

namespace Training.SkillDevelopment.Tests.Unit.Provider;

public sealed class TrainingProviderServiceTests
{
    private readonly Mock<ITrainingProviderRepository> _repositoryMock;
    private readonly Mock<ILogger<TrainingProviderService>> _loggerMock;
    private readonly TrainingProviderService _sut;

    public TrainingProviderServiceTests()
    {
        _repositoryMock = new Mock<ITrainingProviderRepository>();
        _loggerMock = new Mock<ILogger<TrainingProviderService>>();
        _sut = new TrainingProviderService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidProvider_ReturnsCreatedProvider()
    {
        // Arrange
        var provider = BuildValidProvider();
        var created = BuildValidProvider();
        created.Id = Guid.NewGuid().ToString();
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TrainingProvider>()))
            .ReturnsAsync(created);

        // Act
        var result = await _sut.CreateAsync(provider);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateAsync_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var provider = BuildValidProvider();
        provider.Name = string.Empty;

        // Act
        var act = async () => await _sut.CreateAsync(provider);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*nom*");
    }

    [Fact]
    public async Task CreateAsync_MissingDeclarationNumber_ThrowsArgumentException()
    {
        // Arrange
        var provider = BuildValidProvider();
        provider.DeclarationNumber = null;

        // Act
        var act = async () => await _sut.CreateAsync(provider);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*NDA*");
    }

    [Fact]
    public async Task CreateAsync_ExpiredQualiopiCertification_ThrowsArgumentException()
    {
        // Arrange
        var provider = BuildValidProvider();
        provider.HasQualiopiCertification = true;
        provider.QualiopiExpirationDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var act = async () => await _sut.CreateAsync(provider);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Qualiopi*");
    }

    [Fact]
    public async Task CreateAsync_ValidQualiopiCertification_CreatesSuccessfully()
    {
        // Arrange
        var provider = BuildValidProvider();
        provider.HasQualiopiCertification = true;
        provider.QualiopiExpirationDate = DateTime.UtcNow.AddYears(1);
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TrainingProvider>()))
            .ReturnsAsync((TrainingProvider p) => p);

        // Act
        var result = await _sut.CreateAsync(provider);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ValidateQualiopiAsync_ProviderWithValidCertification_ReturnsTrue()
    {
        // Arrange
        var provider = BuildValidProvider();
        provider.Id = "provider-001";
        provider.HasQualiopiCertification = true;
        provider.QualiopiExpirationDate = DateTime.UtcNow.AddYears(1);
        _repositoryMock
            .Setup(r => r.GetByIdAsync("provider-001"))
            .ReturnsAsync(provider);

        // Act
        var result = await _sut.ValidateQualiopiAsync("provider-001");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateQualiopiAsync_ProviderWithoutCertification_ReturnsFalse()
    {
        // Arrange
        var provider = BuildValidProvider();
        provider.Id = "provider-001";
        provider.HasQualiopiCertification = false;
        _repositoryMock
            .Setup(r => r.GetByIdAsync("provider-001"))
            .ReturnsAsync(provider);

        // Act
        var result = await _sut.ValidateQualiopiAsync("provider-001");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateQualiopiAsync_ExpiredCertification_ReturnsFalse()
    {
        // Arrange
        var provider = BuildValidProvider();
        provider.Id = "provider-001";
        provider.HasQualiopiCertification = true;
        provider.QualiopiExpirationDate = DateTime.UtcNow.AddDays(-30);
        _repositoryMock
            .Setup(r => r.GetByIdAsync("provider-001"))
            .ReturnsAsync(provider);

        // Act
        var result = await _sut.ValidateQualiopiAsync("provider-001");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateQualiopiAsync_NonExistentProvider_ThrowsKeyNotFoundException()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync("not-found"))
            .ReturnsAsync((TrainingProvider?)null);

        // Act
        var act = async () => await _sut.ValidateQualiopiAsync("not-found");

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetProviderRatingAsync_ExistingProvider_ReturnsRating()
    {
        // Arrange
        var provider = BuildValidProvider();
        provider.Id = "provider-001";
        _repositoryMock
            .Setup(r => r.GetByIdAsync("provider-001"))
            .ReturnsAsync(provider);

        // Act
        var result = await _sut.GetProviderRatingAsync("provider-001");

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0m);
    }

    [Fact]
    public async Task GetProviderRatingAsync_NonExistentProvider_ThrowsKeyNotFoundException()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync("not-found"))
            .ReturnsAsync((TrainingProvider?)null);

        // Act
        var act = async () => await _sut.GetProviderRatingAsync("not-found");

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task SearchByNameAsync_EmptySearchTerm_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.SearchByNameAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*recherche*");
    }

    [Fact]
    public async Task UpdateAsync_NonExistentProvider_ThrowsKeyNotFoundException()
    {
        // Arrange
        var provider = BuildValidProvider();
        provider.Id = "not-found";
        _repositoryMock
            .Setup(r => r.GetByIdAsync("not-found"))
            .ReturnsAsync((TrainingProvider?)null);

        // Act
        var act = async () => await _sut.UpdateAsync(provider);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ValidId_CallsRepositoryDelete()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.DeleteAsync("provider-001"))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync("provider-001");

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync("provider-001"), Times.Once);
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

    private static TrainingProvider BuildValidProvider() =>
        new()
        {
            Name = "CFA Formation Pro",
            DeclarationNumber = "11756xxxxxx",
            HasQualiopiCertification = true,
            QualiopiExpirationDate = DateTime.UtcNow.AddYears(2),
            IsActive = true
        };
}
