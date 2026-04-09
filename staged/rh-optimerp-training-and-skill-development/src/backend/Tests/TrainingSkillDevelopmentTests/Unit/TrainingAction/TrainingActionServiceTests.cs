using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Repositories.FormationExecution.TrainingAction;
using Training.SkillDevelopment.Services.FormationExecution.TrainingAction;
using Xunit;
using TrainingActionModel = Training.SkillDevelopment.Models.FormationExecution.TrainingAction.TrainingAction;

namespace Training.SkillDevelopment.Tests.Unit.TrainingAction;

public sealed class TrainingActionServiceTests
{
    private readonly Mock<ITrainingActionRepository> _repositoryMock;
    private readonly Mock<ILogger<TrainingActionService>> _loggerMock;
    private readonly TrainingActionService _sut;

    public TrainingActionServiceTests()
    {
        _repositoryMock = new Mock<ITrainingActionRepository>();
        _loggerMock = new Mock<ILogger<TrainingActionService>>();
        _sut = new TrainingActionService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidAction_ReturnsCreated()
    {
        // Arrange
        var action = BuildValidAction();
        var created = BuildValidAction();
        created.Id = Guid.NewGuid().ToString();
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TrainingActionModel>()))
            .ReturnsAsync(created);

        // Act
        var result = await _sut.CreateAsync(action);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_EmptyTitle_ThrowsArgumentException()
    {
        // Arrange
        var action = BuildValidAction();
        action.Title = string.Empty;

        // Act
        var act = async () => await _sut.CreateAsync(action);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateAsync_ZeroDuration_ThrowsArgumentException()
    {
        // Arrange
        var action = BuildValidAction();
        action.DurationHours = 0;

        // Act
        var act = async () => await _sut.CreateAsync(action);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateAsync_NegativeCost_ThrowsArgumentException()
    {
        // Arrange
        var action = BuildValidAction();
        action.Cost = -100m;

        // Act
        var act = async () => await _sut.CreateAsync(action);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsAction()
    {
        // Arrange
        var expected = BuildValidAction();
        expected.Id = "action-001";
        _repositoryMock
            .Setup(r => r.GetByIdAsync("action-001"))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.GetByIdAsync("action-001");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync("unknown"))
            .ReturnsAsync((TrainingActionModel?)null);

        // Act
        var result = await _sut.GetByIdAsync("unknown");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByPlanIdAsync_ValidPlanId_ReturnsActions()
    {
        // Arrange
        var actions = new List<TrainingActionModel> { BuildValidAction(), BuildValidAction() };
        _repositoryMock
            .Setup(r => r.GetByPlanIdAsync("plan-001"))
            .ReturnsAsync(actions);

        // Act
        var result = await _sut.GetByPlanIdAsync("plan-001");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_ValidAction_ReturnsUpdated()
    {
        // Arrange
        var existing = BuildValidAction();
        existing.Id = "action-001";
        var update = BuildValidAction();
        update.Id = "action-001";
        update.Title = "Updated Title";
        _repositoryMock
            .Setup(r => r.GetByIdAsync("action-001"))
            .ReturnsAsync(existing);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<TrainingActionModel>()))
            .ReturnsAsync(update);

        // Act
        var result = await _sut.UpdateAsync(update);

        // Assert
        result.Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_DeletesAction()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.DeleteAsync("action-001"))
            .Returns(Task.CompletedTask);

        // Act
        var act = async () => await _sut.DeleteAsync("action-001");

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetPagedAsync_Page1_ReturnsFirstPage()
    {
        // Arrange
        var pagedResult = new PagedResult<TrainingActionModel>
        {
            Items = new List<TrainingActionModel> { BuildValidAction() },
            TotalCount = 1,
            Page = 1,
            PageSize = 20
        };
        _repositoryMock
            .Setup(r => r.GetPagedAsync(1, 20))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _sut.GetPagedAsync(1, 20);

        // Assert
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetPagedAsync_InvalidPage_ThrowsArgumentException()
    {
        // Arrange
        // Page 0 is invalid (must be >= 1)

        // Act
        var act = async () => await _sut.GetPagedAsync(0, 20);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateAsync_ObligatoryAction_SetsIsObligatoryTrue()
    {
        // Arrange
        var action = BuildValidAction();
        action.IsObligatory = true;
        TrainingActionModel? captured = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TrainingActionModel>()))
            .Callback<TrainingActionModel>(a => captured = a)
            .ReturnsAsync((TrainingActionModel a) => a);

        // Act
        await _sut.CreateAsync(action);

        // Assert
        captured!.IsObligatory.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_NonExistentId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var action = BuildValidAction();
        action.Id = "not-found";
        _repositoryMock
            .Setup(r => r.GetByIdAsync("not-found"))
            .ReturnsAsync((TrainingActionModel?)null);

        // Act
        var act = async () => await _sut.UpdateAsync(action);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    private static TrainingActionModel BuildValidAction() =>
        new()
        {
            Title = "Formation Securite au Travail",
            DurationHours = 8,
            Cost = 1200m,
            MaxParticipants = 15,
            Type = TrainingActionType.Obligatoire
        };
}
