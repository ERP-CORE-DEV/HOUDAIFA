using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.Competency;
using Training.SkillDevelopment.Repositories.Competency;
using Training.SkillDevelopment.Repositories.FormationExecution.TrainingAction;
using Training.SkillDevelopment.Services.CareerGuidance;
using Xunit;
using CompetencyModel = Training.SkillDevelopment.Models.Competency.Competency;
using TrainingActionEntity = Training.SkillDevelopment.Models.FormationExecution.TrainingAction.TrainingAction;

namespace Training.SkillDevelopment.Tests.Unit.CareerGuidance;

public sealed class CareerGuidanceServiceTests
{
    private readonly Mock<ICompetencyRepository> _competencyRepositoryMock;
    private readonly Mock<ICompetencyAssessmentRepository> _assessmentRepositoryMock;
    private readonly Mock<ITrainingActionRepository> _actionRepositoryMock;
    private readonly Mock<ILogger<CareerGuidanceService>> _loggerMock;
    private readonly CareerGuidanceService _sut;

    public CareerGuidanceServiceTests()
    {
        _competencyRepositoryMock = new Mock<ICompetencyRepository>();
        _assessmentRepositoryMock = new Mock<ICompetencyAssessmentRepository>();
        _actionRepositoryMock = new Mock<ITrainingActionRepository>();
        _loggerMock = new Mock<ILogger<CareerGuidanceService>>();

        _sut = new CareerGuidanceService(
            _competencyRepositoryMock.Object,
            _assessmentRepositoryMock.Object,
            _actionRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetTrainingRecommendationsAsync_EmptyEmployeeId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.GetTrainingRecommendationsAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*employe*");
    }

    [Fact]
    public async Task GetTrainingRecommendationsAsync_WhitespaceEmployeeId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.GetTrainingRecommendationsAsync("   ");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetTrainingRecommendationsAsync_NoGaps_ReturnsEmptyList()
    {
        // Arrange
        _assessmentRepositoryMock
            .Setup(r => r.GetGapsAsync("emp-001", default))
            .ReturnsAsync(new List<CompetencyGap>());

        // Act
        var result = await _sut.GetTrainingRecommendationsAsync("emp-001");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTrainingRecommendationsAsync_GapWithMatchingAction_ReturnsRecommendation()
    {
        // Arrange
        var competency = BuildCompetency("comp-001", "C# Avance");
        var gap = BuildGap("emp-001", "comp-001", CompetencyLevel.Pratique, CompetencyLevel.Expert);

        _assessmentRepositoryMock
            .Setup(r => r.GetGapsAsync("emp-001", default))
            .ReturnsAsync(new List<CompetencyGap> { gap });

        _competencyRepositoryMock
            .Setup(r => r.GetByIdAsync("comp-001", default))
            .ReturnsAsync(competency);

        var action = BuildAction("action-001", "Formation C# Expert", new[] { "comp-001" });
        _actionRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<TrainingActionEntity> { action });

        // Act
        var result = await _sut.GetTrainingRecommendationsAsync("emp-001");

        // Assert
        result.Should().HaveCount(1);
        result[0].EmployeeId.Should().Be("emp-001");
        result[0].CompetencyName.Should().Be("C# Avance");
        result[0].GapSize.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetTrainingRecommendationsAsync_MultipleGaps_OrdersByGapSizeDescending()
    {
        // Arrange
        var comp1 = BuildCompetency("comp-001", "Competence A");
        var comp2 = BuildCompetency("comp-002", "Competence B");

        var smallGap = BuildGap("emp-001", "comp-001", CompetencyLevel.Confirme, CompetencyLevel.Expert);
        var bigGap = BuildGap("emp-001", "comp-002", CompetencyLevel.Initie, CompetencyLevel.Expert);

        _assessmentRepositoryMock
            .Setup(r => r.GetGapsAsync("emp-001", default))
            .ReturnsAsync(new List<CompetencyGap> { smallGap, bigGap });

        _competencyRepositoryMock
            .Setup(r => r.GetByIdAsync("comp-001", default))
            .ReturnsAsync(comp1);
        _competencyRepositoryMock
            .Setup(r => r.GetByIdAsync("comp-002", default))
            .ReturnsAsync(comp2);

        _actionRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<TrainingActionEntity>());

        // Act
        var result = await _sut.GetTrainingRecommendationsAsync("emp-001");

        // Assert
        result.Should().HaveCount(2);
        result[0].GapSize.Should().BeGreaterThanOrEqualTo(result[1].GapSize);
    }

    [Fact]
    public async Task GetTrainingRecommendationsAsync_GapWithNoMatchingAction_ReturnsRecommendationWithEmptyActionId()
    {
        // Arrange
        var competency = BuildCompetency("comp-001", "Competence Rare");
        var gap = BuildGap("emp-001", "comp-001", CompetencyLevel.Pratique, CompetencyLevel.Confirme);

        _assessmentRepositoryMock
            .Setup(r => r.GetGapsAsync("emp-001", default))
            .ReturnsAsync(new List<CompetencyGap> { gap });

        _competencyRepositoryMock
            .Setup(r => r.GetByIdAsync("comp-001", default))
            .ReturnsAsync(competency);

        _actionRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<TrainingActionEntity>());

        // Act
        var result = await _sut.GetTrainingRecommendationsAsync("emp-001");

        // Assert
        result.Should().HaveCount(1);
        result[0].RecommendedTrainingActionId.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTrainingRecommendationsAsync_GapWithZeroGapSize_IsExcluded()
    {
        // Arrange
        var gap = new CompetencyGap
        {
            Id = "gap-001",
            EmployeeId = "emp-001",
            CompetencyId = "comp-001",
            RequiredLevel = CompetencyLevel.Pratique,
            CurrentLevel = CompetencyLevel.Pratique,
            Priority = 1
        };

        _assessmentRepositoryMock
            .Setup(r => r.GetGapsAsync("emp-001", default))
            .ReturnsAsync(new List<CompetencyGap> { gap });

        // Act
        var result = await _sut.GetTrainingRecommendationsAsync("emp-001");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetJobTransitionPathsAsync_EmptyRoleId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.GetJobTransitionPathsAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*poste*");
    }

    [Fact]
    public async Task GetJobTransitionPathsAsync_NoCriticalCompetencies_ReturnsEmptyList()
    {
        // Arrange
        _competencyRepositoryMock
            .Setup(r => r.GetByCriticalAsync(default))
            .ReturnsAsync(new List<CompetencyModel>());

        // Act
        var result = await _sut.GetJobTransitionPathsAsync("role-001");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetJobTransitionPathsAsync_WithActionsAndCriticalCompetencies_ReturnsPaths()
    {
        // Arrange
        var criticalCompetencies = new List<CompetencyModel>
        {
            BuildCompetency("comp-001", "Management", isCritical: true)
        };
        _competencyRepositoryMock
            .Setup(r => r.GetByCriticalAsync(default))
            .ReturnsAsync(criticalCompetencies);

        var actions = new List<TrainingActionEntity>
        {
            BuildAction("action-001", "Formation Leadership", null, "Management"),
            BuildAction("action-002", "Formation Technique", null, "Informatique")
        };
        _actionRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(actions);

        // Act
        var result = await _sut.GetJobTransitionPathsAsync("ROLE-AUTRE");

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetJobTransitionPathsAsync_PathsOrderedByMatchPercentageDescending()
    {
        // Arrange
        var criticalCompetencies = new List<CompetencyModel>
        {
            BuildCompetency("comp-001", "Management", isCritical: true)
        };
        _competencyRepositoryMock
            .Setup(r => r.GetByCriticalAsync(default))
            .ReturnsAsync(criticalCompetencies);

        var actions = new List<TrainingActionEntity>
        {
            BuildAction("action-001", "Formation A", null, "Management"),
            BuildAction("action-002", "Formation B", null, "Informatique"),
            BuildAction("action-003", "Formation C", null, "Finance")
        };
        _actionRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(actions);

        // Act
        var result = await _sut.GetJobTransitionPathsAsync("ROLE-AUTRE");

        // Assert
        for (var i = 0; i < result.Count - 1; i++)
        {
            result[i].MatchPercentage.Should().BeGreaterThanOrEqualTo(result[i + 1].MatchPercentage);
        }
    }

    [Fact]
    public async Task GetJobTransitionPathsAsync_ExcludesCurrentRole()
    {
        // Arrange
        var criticalCompetencies = new List<CompetencyModel>
        {
            BuildCompetency("comp-001", "Management", isCritical: true)
        };
        _competencyRepositoryMock
            .Setup(r => r.GetByCriticalAsync(default))
            .ReturnsAsync(criticalCompetencies);

        var actions = new List<TrainingActionEntity>
        {
            BuildAction("action-001", "Formation Management", null, "Management")
        };
        _actionRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(actions);

        // Act
        var result = await _sut.GetJobTransitionPathsAsync("ROLE-MANAGEMENT");

        // Assert
        result.Should().NotContain(p => p.TargetRoleId == "ROLE-MANAGEMENT");
    }

    [Fact]
    public async Task GetJobTransitionPathsAsync_EachPathHasRequiredTrainings()
    {
        // Arrange
        var criticalCompetencies = new List<CompetencyModel>
        {
            BuildCompetency("comp-001", "Finance", isCritical: true)
        };
        _competencyRepositoryMock
            .Setup(r => r.GetByCriticalAsync(default))
            .ReturnsAsync(criticalCompetencies);

        var actions = new List<TrainingActionEntity>
        {
            BuildAction("action-001", "Comptabilite Avancee", null, "Finance"),
            BuildAction("action-002", "Gestion Budgetaire", null, "Finance")
        };
        _actionRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(actions);

        // Act
        var result = await _sut.GetJobTransitionPathsAsync("ROLE-AUTRE");

        // Assert
        result.Should().NotBeEmpty();
        result.All(p => p.RequiredTrainings is not null).Should().BeTrue();
    }

    private static CompetencyModel BuildCompetency(string id, string name, bool isCritical = false) =>
        new()
        {
            Id = id,
            Code = $"CODE-{id}",
            Name = name,
            Description = $"Description de {name}",
            Domain = name,
            Family = "Transverse",
            Type = CompetencyType.Technical,
            IsCritical = isCritical,
            IsActive = true
        };

    private static CompetencyGap BuildGap(
        string employeeId,
        string competencyId,
        CompetencyLevel current,
        CompetencyLevel required) =>
        new()
        {
            Id = Guid.NewGuid().ToString(),
            EmployeeId = employeeId,
            CompetencyId = competencyId,
            CurrentLevel = current,
            RequiredLevel = required,
            Priority = 1
        };

    private static TrainingActionEntity BuildAction(
        string id,
        string title,
        string[]? competencyIds,
        string? category = null) =>
        new()
        {
            Id = id,
            Title = title,
            CompetencyIds = competencyIds,
            Category = category,
            IsActive = true,
            DurationHours = 8,
            Cost = 500m,
            MaxParticipants = 10,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
}
