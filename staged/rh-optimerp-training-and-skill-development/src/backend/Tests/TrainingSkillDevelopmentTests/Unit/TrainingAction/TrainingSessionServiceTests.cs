using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FormationExecution.TrainingAction;
using Training.SkillDevelopment.Repositories.FormationExecution.TrainingAction;
using Training.SkillDevelopment.Services.FormationExecution.TrainingAction;
using Xunit;

namespace Training.SkillDevelopment.Tests.Unit.TrainingAction;

public sealed class TrainingSessionServiceTests
{
    private readonly Mock<ITrainingSessionRepository> _repositoryMock;
    private readonly Mock<ILogger<TrainingSessionService>> _loggerMock;
    private readonly TrainingSessionService _sut;

    public TrainingSessionServiceTests()
    {
        _repositoryMock = new Mock<ITrainingSessionRepository>();
        _loggerMock = new Mock<ILogger<TrainingSessionService>>();
        _sut = new TrainingSessionService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidSession_ReturnsCreated()
    {
        // Arrange
        var session = BuildValidSession();
        var created = BuildValidSession();
        created.Id = Guid.NewGuid().ToString();
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TrainingSession>()))
            .ReturnsAsync(created);

        // Act
        var result = await _sut.CreateAsync(session);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_EndBeforeStart_ThrowsArgumentException()
    {
        // Arrange
        var session = BuildValidSession();
        session.StartDate = DateTime.UtcNow.AddDays(5);
        session.EndDate = DateTime.UtcNow.AddDays(1);

        // Act
        var act = async () => await _sut.CreateAsync(session);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateAsync_ZeroCapacity_ThrowsArgumentException()
    {
        // Arrange
        var session = BuildValidSession();
        session.MaxCapacity = 0;

        // Act
        var act = async () => await _sut.CreateAsync(session);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsSession()
    {
        // Arrange
        var expected = BuildValidSession();
        expected.Id = "session-001";
        _repositoryMock
            .Setup(r => r.GetByIdAsync("session-001"))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.GetByIdAsync("session-001");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByActionIdAsync_ValidActionId_ReturnsSessions()
    {
        // Arrange
        var sessions = new List<TrainingSession> { BuildValidSession(), BuildValidSession() };
        _repositoryMock
            .Setup(r => r.GetByActionIdAsync("action-001"))
            .ReturnsAsync(sessions);

        // Act
        var result = await _sut.GetByActionIdAsync("action-001");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByDateRangeAsync_ValidRange_ReturnsFilteredSessions()
    {
        // Arrange
        var start = DateTime.UtcNow;
        var end = start.AddDays(30);
        var sessions = new List<TrainingSession> { BuildValidSession() };
        _repositoryMock
            .Setup(r => r.GetByDateRangeAsync(start, end))
            .ReturnsAsync(sessions);

        // Act
        var result = await _sut.GetByDateRangeAsync(start, end);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateAsync_ValidSession_ReturnsUpdated()
    {
        // Arrange
        var existing = BuildValidSession();
        existing.Id = "session-001";
        var update = BuildValidSession();
        update.Id = "session-001";
        update.Location = "Salle B";
        _repositoryMock
            .Setup(r => r.GetByIdAsync("session-001"))
            .ReturnsAsync(existing);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<TrainingSession>()))
            .ReturnsAsync(update);

        // Act
        var result = await _sut.UpdateAsync(update);

        // Assert
        result.Location.Should().Be("Salle B");
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_DeletesSession()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.DeleteAsync("session-001"))
            .Returns(Task.CompletedTask);

        // Act
        var act = async () => await _sut.DeleteAsync("session-001");

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CreateAsync_ValidDates_SetsStatusToPlanned()
    {
        // Arrange
        var session = BuildValidSession();
        TrainingSession? captured = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TrainingSession>()))
            .Callback<TrainingSession>(s => captured = s)
            .ReturnsAsync((TrainingSession s) => s);

        // Act
        await _sut.CreateAsync(session);

        // Assert
        captured!.Status.Should().Be(SessionStatus.Planned);
    }

    [Fact]
    public async Task GetByActionIdAsync_NoSessions_ReturnsEmptyList()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByActionIdAsync("action-empty"))
            .ReturnsAsync(new List<TrainingSession>());

        // Act
        var result = await _sut.GetByActionIdAsync("action-empty");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_NullActionId_ThrowsArgumentException()
    {
        // Arrange
        var session = BuildValidSession();
        session.ActionId = string.Empty;

        // Act
        var act = async () => await _sut.CreateAsync(session);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateAsync_NonExistentSession_ThrowsKeyNotFoundException()
    {
        // Arrange
        var session = BuildValidSession();
        session.Id = "not-found";
        _repositoryMock
            .Setup(r => r.GetByIdAsync("not-found"))
            .ReturnsAsync((TrainingSession?)null);

        // Act
        var act = async () => await _sut.UpdateAsync(session);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetByDateRangeAsync_EndBeforeStart_ThrowsArgumentException()
    {
        // Arrange
        var start = DateTime.UtcNow.AddDays(10);
        var end = DateTime.UtcNow;

        // Act
        var act = async () => await _sut.GetByDateRangeAsync(start, end);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    private static TrainingSession BuildValidSession() =>
        new()
        {
            ActionId = "action-001",
            StartDate = DateTime.UtcNow.AddDays(7),
            EndDate = DateTime.UtcNow.AddDays(8),
            MaxCapacity = 20,
            Location = "Salle A"
        };
}
