using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Training.SkillDevelopment.Models.CertificationEcosysteme.Certification;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Repositories.CertificationEcosysteme.Certification;
using Training.SkillDevelopment.Services.CertificationEcosysteme.Certification;
using Xunit;

namespace Training.SkillDevelopment.Tests.Unit.Certification;

public sealed class VaeProjectServiceTests
{
    private readonly Mock<IVaeProjectRepository> _repositoryMock;
    private readonly Mock<ILogger<VaeProjectService>> _loggerMock;
    private readonly VaeProjectService _sut;

    public VaeProjectServiceTests()
    {
        _repositoryMock = new Mock<IVaeProjectRepository>();
        _loggerMock = new Mock<ILogger<VaeProjectService>>();
        _sut = new VaeProjectService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidProject_SetsRecevabilitePhase()
    {
        // Arrange
        var project = BuildValidProject();
        VaeProject? captured = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<VaeProject>()))
            .Callback<VaeProject>(p => captured = p)
            .ReturnsAsync((VaeProject p) => p);

        // Act
        await _sut.CreateAsync(project);

        // Assert
        captured!.Status.Should().Be(VaeStatus.Recevabilite);
    }

    [Fact]
    public async Task CreateAsync_MissingEmployeeId_ThrowsArgumentException()
    {
        // Arrange
        var project = BuildValidProject();
        project.EmployeeId = string.Empty;

        // Act
        var act = async () => await _sut.CreateAsync(project);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*employe*");
    }

    [Fact]
    public async Task CreateAsync_MissingRncpCodeAndCertificationId_ThrowsArgumentException()
    {
        // Arrange
        var project = BuildValidProject();
        project.RncpCode = null;
        project.CertificationId = null;

        // Act
        var act = async () => await _sut.CreateAsync(project);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*RNCP*");
    }

    [Fact]
    public async Task CreateAsync_WithCertificationId_CreatesSuccessfully()
    {
        // Arrange
        var project = BuildValidProject();
        project.RncpCode = null;
        project.CertificationId = "cert-001";
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<VaeProject>()))
            .ReturnsAsync((VaeProject p) => p);

        // Act
        var result = await _sut.CreateAsync(project);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task AdvancePhaseAsync_FromRecevabilite_MovesToAccompagnement()
    {
        // Arrange
        var project = BuildValidProject();
        project.Id = "vae-001";
        project.Status = VaeStatus.Recevabilite;
        var updated = BuildValidProject();
        updated.Id = "vae-001";
        updated.Status = VaeStatus.Accompagnement;
        _repositoryMock
            .Setup(r => r.GetByIdAsync("vae-001"))
            .ReturnsAsync(project);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<VaeProject>()))
            .ReturnsAsync(updated);

        // Act
        var result = await _sut.AdvancePhaseAsync("vae-001");

        // Assert
        result.Status.Should().Be(VaeStatus.Accompagnement);
    }

    [Fact]
    public async Task AdvancePhaseAsync_FromAccompagnement_MovesToLivret2()
    {
        // Arrange
        var project = BuildValidProject();
        project.Id = "vae-001";
        project.Status = VaeStatus.Accompagnement;
        var updated = BuildValidProject();
        updated.Id = "vae-001";
        updated.Status = VaeStatus.Livret2;
        _repositoryMock
            .Setup(r => r.GetByIdAsync("vae-001"))
            .ReturnsAsync(project);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<VaeProject>()))
            .ReturnsAsync(updated);

        // Act
        var result = await _sut.AdvancePhaseAsync("vae-001");

        // Assert
        result.Status.Should().Be(VaeStatus.Livret2);
    }

    [Fact]
    public async Task AdvancePhaseAsync_FromLivret2_MovesToJury()
    {
        // Arrange
        var project = BuildValidProject();
        project.Id = "vae-001";
        project.Status = VaeStatus.Livret2;
        var updated = BuildValidProject();
        updated.Id = "vae-001";
        updated.Status = VaeStatus.Jury;
        _repositoryMock
            .Setup(r => r.GetByIdAsync("vae-001"))
            .ReturnsAsync(project);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<VaeProject>()))
            .ReturnsAsync(updated);

        // Act
        var result = await _sut.AdvancePhaseAsync("vae-001");

        // Assert
        result.Status.Should().Be(VaeStatus.Jury);
    }

    [Fact]
    public async Task AdvancePhaseAsync_TerminalPhase_ThrowsInvalidOperationException()
    {
        // Arrange
        var project = BuildValidProject();
        project.Id = "vae-001";
        project.Status = VaeStatus.ValidationTotale;
        _repositoryMock
            .Setup(r => r.GetByIdAsync("vae-001"))
            .ReturnsAsync(project);

        // Act
        var act = async () => await _sut.AdvancePhaseAsync("vae-001");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*terminale*");
    }

    [Fact]
    public async Task AdvancePhaseAsync_NonExistentProject_ThrowsKeyNotFoundException()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync("not-found"))
            .ReturnsAsync((VaeProject?)null);

        // Act
        var act = async () => await _sut.AdvancePhaseAsync("not-found");

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
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
    public async Task GetByEmployeeIdAsync_EmptyEmployeeId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.GetByEmployeeIdAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*employe*");
    }

    [Fact]
    public async Task GetByEmployeeIdAsync_ValidEmployeeId_ReturnsProjects()
    {
        // Arrange
        var projects = new List<VaeProject> { BuildValidProject() };
        _repositoryMock
            .Setup(r => r.GetByEmployeeIdAsync("emp-001"))
            .ReturnsAsync(projects);

        // Act
        var result = await _sut.GetByEmployeeIdAsync("emp-001");

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentProject_ThrowsKeyNotFoundException()
    {
        // Arrange
        var project = BuildValidProject();
        project.Id = "not-found";
        _repositoryMock
            .Setup(r => r.GetByIdAsync("not-found"))
            .ReturnsAsync((VaeProject?)null);

        // Act
        var act = async () => await _sut.UpdateAsync(project);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not-found*");
    }

    [Fact]
    public async Task DeleteAsync_ValidId_CallsRepositoryDelete()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.DeleteAsync("vae-001"))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync("vae-001");

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync("vae-001"), Times.Once);
    }

    private static VaeProject BuildValidProject() =>
        new()
        {
            EmployeeId = "emp-001",
            RncpCode = "RNCP38654",
            Status = VaeStatus.Recevabilite
        };
}
