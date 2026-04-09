using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FinancementConformite.Opco;
using Training.SkillDevelopment.Repositories.FinancementConformite.Opco;
using Training.SkillDevelopment.Services.FinancementConformite.Opco;
using Xunit;

namespace Training.SkillDevelopment.Tests.Unit.Opco;

public sealed class FundingApplicationServiceTests
{
    private readonly Mock<IFundingApplicationRepository> _repositoryMock;
    private readonly Mock<ILogger<FundingApplicationService>> _loggerMock;
    private readonly FundingApplicationService _sut;

    public FundingApplicationServiceTests()
    {
        _repositoryMock = new Mock<IFundingApplicationRepository>();
        _loggerMock = new Mock<ILogger<FundingApplicationService>>();
        _sut = new FundingApplicationService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SubmitAsync_ValidApplication_ReturnsSubmittedApplication()
    {
        // Arrange
        var application = BuildValidApplication();
        var created = BuildValidApplication();
        created.Id = Guid.NewGuid().ToString();
        created.Status = FundingStatus.Submitted;
        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<FundingApplication>(), default))
            .ReturnsAsync(created);

        // Act
        var result = await _sut.SubmitAsync(application);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(FundingStatus.Submitted);
    }

    [Fact]
    public async Task SubmitAsync_ZeroRequestedAmount_ThrowsArgumentException()
    {
        // Arrange
        var application = BuildValidApplication();
        application.RequestedAmount = 0m;

        // Act
        var act = async () => await _sut.SubmitAsync(application);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*montant demande*");
    }

    [Fact]
    public async Task SubmitAsync_NegativeRequestedAmount_ThrowsArgumentException()
    {
        // Arrange
        var application = BuildValidApplication();
        application.RequestedAmount = -500m;

        // Act
        var act = async () => await _sut.SubmitAsync(application);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SubmitAsync_EmptyEmployeeIds_ThrowsArgumentException()
    {
        // Arrange
        var application = BuildValidApplication();
        application.EmployeeIds = Array.Empty<string>();

        // Act
        var act = async () => await _sut.SubmitAsync(application);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*employe*");
    }

    [Fact]
    public async Task SubmitAsync_MissingOpcoId_ThrowsArgumentException()
    {
        // Arrange
        var application = BuildValidApplication();
        application.OpcoId = string.Empty;

        // Act
        var act = async () => await _sut.SubmitAsync(application);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*OPCO*");
    }

    [Fact]
    public async Task SubmitAsync_MissingTrainingActionId_ThrowsArgumentException()
    {
        // Arrange
        var application = BuildValidApplication();
        application.TrainingActionId = string.Empty;

        // Act
        var act = async () => await _sut.SubmitAsync(application);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*action de formation*");
    }

    [Fact]
    public async Task ApproveAsync_SubmittedApplication_ReturnsApprovedStatus()
    {
        // Arrange
        var application = BuildValidApplication();
        application.Id = "app-001";
        application.Status = FundingStatus.Submitted;
        var approved = BuildValidApplication();
        approved.Id = "app-001";
        approved.Status = FundingStatus.Approved;
        approved.GrantedAmount = 5000m;
        _repositoryMock
            .Setup(r => r.GetByIdAsync("app-001", default))
            .ReturnsAsync(application);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<FundingApplication>(), default))
            .ReturnsAsync(approved);

        // Act
        var result = await _sut.ApproveAsync("app-001", 5000m);

        // Assert
        result.Status.Should().Be(FundingStatus.Approved);
    }

    [Fact]
    public async Task ApproveAsync_UnderReviewApplication_ReturnsApprovedStatus()
    {
        // Arrange
        var application = BuildValidApplication();
        application.Id = "app-001";
        application.Status = FundingStatus.UnderReview;
        var approved = BuildValidApplication();
        approved.Id = "app-001";
        approved.Status = FundingStatus.Approved;
        _repositoryMock
            .Setup(r => r.GetByIdAsync("app-001", default))
            .ReturnsAsync(application);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<FundingApplication>(), default))
            .ReturnsAsync(approved);

        // Act
        var result = await _sut.ApproveAsync("app-001", 3000m);

        // Assert
        result.Status.Should().Be(FundingStatus.Approved);
    }

    [Fact]
    public async Task ApproveAsync_ZeroGrantedAmount_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.ApproveAsync("app-001", 0m);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*montant accorde*");
    }

    [Fact]
    public async Task ApproveAsync_NonExistentApplication_ThrowsKeyNotFoundException()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync("not-found", default))
            .ReturnsAsync((FundingApplication?)null);

        // Act
        var act = async () => await _sut.ApproveAsync("not-found", 1000m);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not-found*");
    }

    [Fact]
    public async Task RejectAsync_SubmittedApplication_ReturnsRejectedStatus()
    {
        // Arrange
        var application = BuildValidApplication();
        application.Id = "app-001";
        application.Status = FundingStatus.Submitted;
        var rejected = BuildValidApplication();
        rejected.Id = "app-001";
        rejected.Status = FundingStatus.Rejected;
        rejected.RejectionReason = "Budget insuffisant";
        _repositoryMock
            .Setup(r => r.GetByIdAsync("app-001", default))
            .ReturnsAsync(application);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<FundingApplication>(), default))
            .ReturnsAsync(rejected);

        // Act
        var result = await _sut.RejectAsync("app-001", "Budget insuffisant");

        // Assert
        result.Status.Should().Be(FundingStatus.Rejected);
    }

    [Fact]
    public async Task RejectAsync_ApprovedApplication_ThrowsInvalidOperationException()
    {
        // Arrange
        var application = BuildValidApplication();
        application.Id = "app-001";
        application.Status = FundingStatus.Approved;
        _repositoryMock
            .Setup(r => r.GetByIdAsync("app-001", default))
            .ReturnsAsync(application);

        // Act
        var act = async () => await _sut.RejectAsync("app-001", "Annulation");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*approuvee ou payee*");
    }

    [Fact]
    public async Task RejectAsync_EmptyRejectionReason_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.RejectAsync("app-001", string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*motif de rejet*");
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
    public async Task GetByOpcoIdAsync_InvalidPage_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.GetByOpcoIdAsync("opco-001", 0, 20);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*page*");
    }

    private static FundingApplication BuildValidApplication() =>
        new()
        {
            OpcoId = "opco-001",
            TrainingActionId = "action-001",
            EmployeeIds = ["emp-001", "emp-002"],
            RequestedAmount = 5000m,
            Status = FundingStatus.Draft
        };
}
