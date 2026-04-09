using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FormationExecution.Evaluation;
using Training.SkillDevelopment.Repositories.FormationExecution.Evaluation;
using Training.SkillDevelopment.Services.FormationExecution.Evaluation;
using Xunit;

namespace Training.SkillDevelopment.Tests.Unit.Evaluation;

public sealed class TrainingEvaluationServiceTests
{
    private readonly Mock<ITrainingEvaluationRepository> _repositoryMock;
    private readonly Mock<ILogger<TrainingEvaluationService>> _loggerMock;
    private readonly TrainingEvaluationService _sut;

    public TrainingEvaluationServiceTests()
    {
        _repositoryMock = new Mock<ITrainingEvaluationRepository>();
        _loggerMock = new Mock<ILogger<TrainingEvaluationService>>();
        _sut = new TrainingEvaluationService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidEvaluation_ReturnsCreatedEvaluation()
    {
        // Arrange
        var evaluation = BuildValidEvaluation();
        var created = BuildValidEvaluation();
        created.Id = Guid.NewGuid().ToString();
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TrainingEvaluation>()))
            .ReturnsAsync(created);

        // Act
        var result = await _sut.CreateAsync(evaluation);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateAsync_MissingSessionId_ThrowsArgumentException()
    {
        // Arrange
        var evaluation = BuildValidEvaluation();
        evaluation.SessionId = string.Empty;

        // Act
        var act = async () => await _sut.CreateAsync(evaluation);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*session*");
    }

    [Fact]
    public async Task CreateAsync_MissingEmployeeId_ThrowsArgumentException()
    {
        // Arrange
        var evaluation = BuildValidEvaluation();
        evaluation.EmployeeId = string.Empty;

        // Act
        var act = async () => await _sut.CreateAsync(evaluation);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*employe*");
    }

    [Fact]
    public async Task CreateAsync_ScoreExceedsMaxScore_ThrowsArgumentException()
    {
        // Arrange
        var evaluation = BuildValidEvaluation();
        evaluation.Score = 11m;
        evaluation.MaxScore = 10;

        // Act
        var act = async () => await _sut.CreateAsync(evaluation);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*score*");
    }

    [Fact]
    public async Task CreateAsync_NegativeScore_ThrowsArgumentException()
    {
        // Arrange
        var evaluation = BuildValidEvaluation();
        evaluation.Score = -1m;

        // Act
        var act = async () => await _sut.CreateAsync(evaluation);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateAsync_ZeroMaxScore_ThrowsArgumentException()
    {
        // Arrange
        var evaluation = BuildValidEvaluation();
        evaluation.MaxScore = 0;
        evaluation.Score = 0;

        // Act
        var act = async () => await _sut.CreateAsync(evaluation);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*score maximum*");
    }

    [Fact]
    public async Task GetAverageScoreAsync_ValidSessionId_ReturnsAverage()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetAverageScoreAsync("session-001"))
            .ReturnsAsync(7.5m);

        // Act
        var result = await _sut.GetAverageScoreAsync("session-001");

        // Assert
        result.Should().Be(7.5m);
    }

    [Fact]
    public async Task GetAverageScoreAsync_EmptySessionId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.GetAverageScoreAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*session*");
    }

    [Fact]
    public async Task CalculateRoiAsync_WithResultatLevelEvaluations_ReturnsRoiPercentage()
    {
        // Arrange
        var evaluations = new List<TrainingEvaluation>
        {
            new()
            {
                Id = "eval-001",
                SessionId = "session-001",
                EmployeeId = "emp-001",
                Level = EvaluationLevel.Resultats,
                Score = 8m,
                MaxScore = 10
            }
        };
        _repositoryMock
            .Setup(r => r.GetBySessionIdAsync("session-001"))
            .ReturnsAsync(evaluations);

        // Act
        var result = await _sut.CalculateRoiAsync("session-001");

        // Assert
        result.Should().Be(80m);
    }

    [Fact]
    public async Task CalculateRoiAsync_NoResultatLevelEvaluations_ReturnsZero()
    {
        // Arrange
        var evaluations = new List<TrainingEvaluation>
        {
            new()
            {
                Id = "eval-001",
                SessionId = "session-001",
                EmployeeId = "emp-001",
                Level = EvaluationLevel.Satisfaction,
                Score = 8m,
                MaxScore = 10
            }
        };
        _repositoryMock
            .Setup(r => r.GetBySessionIdAsync("session-001"))
            .ReturnsAsync(evaluations);

        // Act
        var result = await _sut.CalculateRoiAsync("session-001");

        // Assert
        result.Should().Be(0m);
    }

    [Fact]
    public async Task GetBySessionIdAsync_EmptySessionId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.GetBySessionIdAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*session*");
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
    public async Task UpdateAsync_NonExistentEvaluation_ThrowsKeyNotFoundException()
    {
        // Arrange
        var evaluation = BuildValidEvaluation();
        evaluation.Id = "not-found";
        _repositoryMock
            .Setup(r => r.GetByIdAsync("not-found"))
            .ReturnsAsync((TrainingEvaluation?)null);

        // Act
        var act = async () => await _sut.UpdateAsync(evaluation);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not-found*");
    }

    [Fact]
    public async Task DeleteAsync_ValidId_CallsRepositoryDelete()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.DeleteAsync("eval-001"))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync("eval-001");

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync("eval-001"), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_KirkpatrickLevel1Satisfaction_CreatesSuccessfully()
    {
        // Arrange
        var evaluation = BuildValidEvaluation();
        evaluation.Level = EvaluationLevel.Satisfaction;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TrainingEvaluation>()))
            .ReturnsAsync((TrainingEvaluation e) => e);

        // Act
        var result = await _sut.CreateAsync(evaluation);

        // Assert
        result.Level.Should().Be(EvaluationLevel.Satisfaction);
    }

    private static TrainingEvaluation BuildValidEvaluation() =>
        new()
        {
            SessionId = "session-001",
            EmployeeId = "emp-001",
            Level = EvaluationLevel.Satisfaction,
            Score = 8m,
            MaxScore = 10
        };
}
