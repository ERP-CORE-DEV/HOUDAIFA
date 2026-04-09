using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FormationExecution.Evaluation;
using Training.SkillDevelopment.Models.FormationExecution.TrainingAction;
using Training.SkillDevelopment.Models.FormationExecution.TrainingPlan;
using Training.SkillDevelopment.Repositories.FinancementConformite.Cpf;
using Training.SkillDevelopment.Repositories.FormationExecution.Evaluation;
using Training.SkillDevelopment.Repositories.FormationExecution.TrainingAction;
using Training.SkillDevelopment.Repositories.FormationExecution.TrainingPlan;
using Training.SkillDevelopment.Services.PilotageGouvernance.Analytics;
using Xunit;
using TrainingActionEntity = Training.SkillDevelopment.Models.FormationExecution.TrainingAction.TrainingAction;
using TrainingPlanModel = Training.SkillDevelopment.Models.FormationExecution.TrainingPlan.TrainingPlan;

namespace Training.SkillDevelopment.Tests.Unit.Analytics;

public sealed class TrainingAnalyticsServiceTests
{
    private readonly Mock<ITrainingPlanRepository> _planRepositoryMock;
    private readonly Mock<ITrainingActionRepository> _actionRepositoryMock;
    private readonly Mock<ITrainingSessionRepository> _sessionRepositoryMock;
    private readonly Mock<IEnrollmentRepository> _enrollmentRepositoryMock;
    private readonly Mock<ITrainingEvaluationRepository> _evaluationRepositoryMock;
    private readonly Mock<ICpfAccountRepository> _cpfAccountRepositoryMock;
    private readonly Mock<ILogger<TrainingAnalyticsService>> _loggerMock;
    private readonly TrainingAnalyticsService _sut;

    public TrainingAnalyticsServiceTests()
    {
        _planRepositoryMock = new Mock<ITrainingPlanRepository>();
        _actionRepositoryMock = new Mock<ITrainingActionRepository>();
        _sessionRepositoryMock = new Mock<ITrainingSessionRepository>();
        _enrollmentRepositoryMock = new Mock<IEnrollmentRepository>();
        _evaluationRepositoryMock = new Mock<ITrainingEvaluationRepository>();
        _cpfAccountRepositoryMock = new Mock<ICpfAccountRepository>();
        _loggerMock = new Mock<ILogger<TrainingAnalyticsService>>();

        _sut = new TrainingAnalyticsService(
            _planRepositoryMock.Object,
            _actionRepositoryMock.Object,
            _sessionRepositoryMock.Object,
            _enrollmentRepositoryMock.Object,
            _evaluationRepositoryMock.Object,
            _cpfAccountRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetDashboardKpisAsync_ValidYear_ReturnsKpiWithCorrectYear()
    {
        // Arrange
        SetupEmptyRepositories(2025);

        // Act
        var result = await _sut.GetDashboardKpisAsync(2025);

        // Assert
        result.Should().NotBeNull();
        result.Year.Should().Be(2025);
    }

    [Fact]
    public async Task GetDashboardKpisAsync_WithPlans_ReturnsTotalPlansCount()
    {
        // Arrange
        var plans = new List<TrainingPlanModel>
        {
            BuildPlan(2025, 10000m, 8000m),
            BuildPlan(2025, 5000m, 4000m)
        };
        _planRepositoryMock.Setup(r => r.GetByYearAsync(2025)).ReturnsAsync(plans);
        _actionRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TrainingActionEntity>());
        _sessionRepositoryMock
            .Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<TrainingSession>());

        // Act
        var result = await _sut.GetDashboardKpisAsync(2025);

        // Assert
        result.TotalTrainingPlans.Should().Be(2);
    }

    [Fact]
    public async Task GetDashboardKpisAsync_WithBudgets_CalculatesBudgetUtilizationRate()
    {
        // Arrange
        var plans = new List<TrainingPlanModel>
        {
            BuildPlan(2025, 10000m, 8000m)
        };
        _planRepositoryMock.Setup(r => r.GetByYearAsync(2025)).ReturnsAsync(plans);
        _actionRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TrainingActionEntity>());
        _sessionRepositoryMock
            .Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<TrainingSession>());

        // Act
        var result = await _sut.GetDashboardKpisAsync(2025);

        // Assert
        result.TotalBudgetAllocated.Should().Be(10000m);
        result.TotalBudgetConsumed.Should().Be(8000m);
        result.BudgetUtilizationRate.Should().Be(80m);
    }

    [Fact]
    public async Task GetDashboardKpisAsync_WithCompletedEnrollments_CalculatesCompletionRate()
    {
        // Arrange
        SetupEmptyRepositories(2025);
        var session = BuildSession("session-001");
        _sessionRepositoryMock
            .Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<TrainingSession> { session });

        var enrollments = new List<Enrollment>
        {
            BuildEnrollment(EnrollmentStatus.Attended),
            BuildEnrollment(EnrollmentStatus.Attended),
            BuildEnrollment(EnrollmentStatus.Cancelled)
        };
        _enrollmentRepositoryMock
            .Setup(r => r.GetBySessionIdAsync("session-001"))
            .ReturnsAsync(enrollments);
        _evaluationRepositoryMock
            .Setup(r => r.GetAverageScoreAsync("session-001"))
            .ReturnsAsync(0m);

        // Act
        var result = await _sut.GetDashboardKpisAsync(2025);

        // Assert
        result.TotalEnrollments.Should().Be(3);
        result.CompletionRate.Should().BeApproximately(66.67m, 0.01m);
    }

    [Fact]
    public async Task GetDashboardKpisAsync_InvalidYear_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = async () => await _sut.GetDashboardKpisAsync(1999);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task GetDashboardKpisAsync_YearTooFarFuture_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = async () => await _sut.GetDashboardKpisAsync(2101);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task GetBilanSocialTrainingAsync_ValidYear_ReturnsCorrectYear()
    {
        // Arrange
        _planRepositoryMock.Setup(r => r.GetByYearAsync(2025)).ReturnsAsync(new List<TrainingPlanModel>());
        _actionRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TrainingActionEntity>());
        _sessionRepositoryMock
            .Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<TrainingSession>());

        // Act
        var result = await _sut.GetBilanSocialTrainingAsync(2025);

        // Assert
        result.Year.Should().Be(2025);
    }

    [Fact]
    public async Task GetBilanSocialTrainingAsync_WithMasseSalariale_CalculatesPercentage()
    {
        // Arrange
        var plan = BuildPlan(2025, 10000m, 2000m);
        plan.MasseSalariale = 100000m;
        _planRepositoryMock.Setup(r => r.GetByYearAsync(2025)).ReturnsAsync(new List<TrainingPlanModel> { plan });
        _actionRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TrainingActionEntity>());
        _sessionRepositoryMock
            .Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<TrainingSession>());

        // Act
        var result = await _sut.GetBilanSocialTrainingAsync(2025);

        // Assert
        result.PercentageOfMasseSalariale.Should().Be(2m);
    }

    [Fact]
    public async Task GetBilanSocialTrainingAsync_WithActions_CalculatesTotalHours()
    {
        // Arrange
        var actions = new List<TrainingActionEntity>
        {
            BuildAction(24, "Informatique", isActive: true),
            BuildAction(16, "Management", isActive: true)
        };
        _planRepositoryMock.Setup(r => r.GetByYearAsync(2025)).ReturnsAsync(new List<TrainingPlanModel>());
        _actionRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(actions);
        _sessionRepositoryMock
            .Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<TrainingSession>());

        // Act
        var result = await _sut.GetBilanSocialTrainingAsync(2025);

        // Assert
        result.TotalTrainingHours.Should().Be(40m);
    }

    [Fact]
    public async Task GetGenderEqualityReportAsync_ValidYear_ReturnsReport()
    {
        // Arrange
        _sessionRepositoryMock
            .Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<TrainingSession>());

        // Act
        var result = await _sut.GetGenderEqualityReportAsync(2025);

        // Assert
        result.Should().NotBeNull();
        result.Year.Should().Be(2025);
    }

    [Fact]
    public async Task GetGenderEqualityReportAsync_NoData_ReturnsZeroRates()
    {
        // Arrange
        _sessionRepositoryMock
            .Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<TrainingSession>());

        // Act
        var result = await _sut.GetGenderEqualityReportAsync(2025);

        // Assert
        result.FemaleTrainingRate.Should().Be(0m);
        result.MaleTrainingRate.Should().Be(0m);
        result.GapPercentage.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateTrainingRoiAsync_EmptyActionId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.CalculateTrainingRoiAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*obligatoire*");
    }

    [Fact]
    public async Task CalculateTrainingRoiAsync_NonExistentAction_ThrowsKeyNotFoundException()
    {
        // Arrange
        _actionRepositoryMock
            .Setup(r => r.GetByIdAsync("not-found"))
            .ReturnsAsync((TrainingActionEntity?)null);

        // Act
        var act = async () => await _sut.CalculateTrainingRoiAsync("not-found");

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CalculateTrainingRoiAsync_NoCost_ReturnsZero()
    {
        // Arrange
        var action = BuildAction(8, "IT", isActive: true);
        action.Id = "action-001";
        action.Cost = 0m;
        _actionRepositoryMock
            .Setup(r => r.GetByIdAsync("action-001"))
            .ReturnsAsync(action);
        _sessionRepositoryMock
            .Setup(r => r.GetByActionIdAsync("action-001"))
            .ReturnsAsync(new List<TrainingSession>());

        // Act
        var result = await _sut.CalculateTrainingRoiAsync("action-001");

        // Assert
        result.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateTrainingRoiAsync_WithLevel4Evaluations_ReturnsRoi()
    {
        // Arrange
        var action = BuildAction(8, "IT", isActive: true);
        action.Id = "action-001";
        action.Cost = 1000m;
        _actionRepositoryMock
            .Setup(r => r.GetByIdAsync("action-001"))
            .ReturnsAsync(action);

        var session = BuildSession("session-001");
        _sessionRepositoryMock
            .Setup(r => r.GetByActionIdAsync("action-001"))
            .ReturnsAsync(new List<TrainingSession> { session });

        var evaluations = new List<TrainingEvaluation>
        {
            new() { Id = "eval-001", Level = EvaluationLevel.Resultats, Score = 8m, MaxScore = 10 }
        };
        _evaluationRepositoryMock
            .Setup(r => r.GetBySessionIdAsync("session-001"))
            .ReturnsAsync(evaluations);

        // Act
        var result = await _sut.CalculateTrainingRoiAsync("action-001");

        // Assert
        result.Should().BeGreaterThan(0m);
    }

    [Fact]
    public async Task GetTrainingTrendsAsync_StartYearAfterEndYear_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.GetTrainingTrendsAsync(2026, 2025);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetTrainingTrendsAsync_RangeExceeds20Years_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.GetTrainingTrendsAsync(2000, 2025);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetTrainingTrendsAsync_ValidRange_ReturnsTrendForEachYear()
    {
        // Arrange
        for (var y = 2023; y <= 2025; y++)
        {
            int year = y;
            _planRepositoryMock.Setup(r => r.GetByYearAsync(year)).ReturnsAsync(new List<TrainingPlanModel>());
            _sessionRepositoryMock
                .Setup(r => r.GetByDateRangeAsync(
                    It.Is<DateTime>(d => d.Year == year),
                    It.Is<DateTime>(d => d.Year == year)))
                .ReturnsAsync(new List<TrainingSession>());
        }
        _actionRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TrainingActionEntity>());

        // Act
        var result = await _sut.GetTrainingTrendsAsync(2023, 2025);

        // Assert
        result.Should().HaveCount(3);
        result.Select(t => t.Year).Should().BeEquivalentTo(new[] { 2023, 2024, 2025 });
    }

    private void SetupEmptyRepositories(int year)
    {
        _planRepositoryMock.Setup(r => r.GetByYearAsync(year)).ReturnsAsync(new List<TrainingPlanModel>());
        _actionRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TrainingActionEntity>());
        _sessionRepositoryMock
            .Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<TrainingSession>());
    }

    private static TrainingPlanModel BuildPlan(int year, decimal allocated, decimal consumed) =>
        new()
        {
            Id = Guid.NewGuid().ToString(),
            CompanyId = "company-001",
            Title = $"Plan {year}",
            Year = year,
            BudgetAllocated = allocated,
            BudgetConsumed = consumed,
            MasseSalariale = 0m,
            IsActive = true,
            Status = TrainingPlanStatus.Approved
        };

    private static TrainingSession BuildSession(string id) =>
        new()
        {
            Id = id,
            ActionId = "action-001",
            StartDate = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2025, 6, 2, 0, 0, 0, DateTimeKind.Utc),
            Status = SessionStatus.Completed,
            IsActive = true
        };

    private static Enrollment BuildEnrollment(EnrollmentStatus status) =>
        new()
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = "session-001",
            EmployeeId = "emp-001",
            Status = status,
            EnrolledAt = DateTime.UtcNow
        };

    private static TrainingActionEntity BuildAction(int hours, string category, bool isActive) =>
        new()
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Formation Test",
            DurationHours = hours,
            Category = category,
            Cost = 1000m,
            IsActive = isActive,
            MaxParticipants = 10,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
}
