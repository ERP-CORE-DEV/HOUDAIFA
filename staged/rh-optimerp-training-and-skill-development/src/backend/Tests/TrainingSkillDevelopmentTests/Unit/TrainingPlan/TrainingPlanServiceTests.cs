using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Repositories.FormationExecution.TrainingPlan;
using Training.SkillDevelopment.Services.FormationExecution.TrainingPlan;
using Xunit;
using TrainingPlanModel = Training.SkillDevelopment.Models.FormationExecution.TrainingPlan.TrainingPlan;

namespace Training.SkillDevelopment.Tests.Unit.TrainingPlan;

public sealed class TrainingPlanServiceTests
{
    private readonly Mock<ITrainingPlanRepository> _repositoryMock;
    private readonly Mock<ILogger<TrainingPlanService>> _loggerMock;
    private readonly TrainingPlanService _sut;

    public TrainingPlanServiceTests()
    {
        _repositoryMock = new Mock<ITrainingPlanRepository>();
        _loggerMock = new Mock<ILogger<TrainingPlanService>>();
        _sut = new TrainingPlanService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidPlan_ReturnsCreatedPlan()
    {
        // Arrange
        var plan = BuildValidPlan();
        var created = BuildValidPlan();
        created.Id = Guid.NewGuid().ToString();
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TrainingPlanModel>()))
            .ReturnsAsync(created);

        // Act
        var result = await _sut.CreateAsync(plan);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_EmptyTitle_ThrowsArgumentException()
    {
        // Arrange
        var plan = BuildValidPlan();
        plan.Title = string.Empty;

        // Act
        var act = async () => await _sut.CreateAsync(plan);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateAsync_NegativeBudget_ThrowsArgumentException()
    {
        // Arrange
        var plan = BuildValidPlan();
        plan.BudgetAllocated = -1m;

        // Act
        var act = async () => await _sut.CreateAsync(plan);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsPlan()
    {
        // Arrange
        var expectedPlan = BuildValidPlan();
        expectedPlan.Id = "plan-001";
        _repositoryMock
            .Setup(r => r.GetByIdAsync("plan-001"))
            .ReturnsAsync(expectedPlan);

        // Act
        var result = await _sut.GetByIdAsync("plan-001");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync("unknown"))
            .ReturnsAsync((TrainingPlanModel?)null);

        // Act
        var result = await _sut.GetByIdAsync("unknown");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByYearAsync_ValidYear_ReturnsPlans()
    {
        // Arrange
        var plans = new List<TrainingPlanModel> { BuildValidPlan() };
        _repositoryMock
            .Setup(r => r.GetByYearAsync(2025))
            .ReturnsAsync(plans);

        // Act
        var result = await _sut.GetByYearAsync(2025);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateAsync_ValidPlan_ReturnsUpdatedPlan()
    {
        // Arrange
        var existing = BuildValidPlan();
        existing.Id = "plan-001";
        existing.Version = 1;
        var update = BuildValidPlan();
        update.Id = "plan-001";
        var updated = BuildValidPlan();
        updated.Id = "plan-001";
        updated.Version = 2;
        _repositoryMock
            .Setup(r => r.GetByIdAsync("plan-001"))
            .ReturnsAsync(existing);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<TrainingPlanModel>()))
            .ReturnsAsync(updated);

        // Act
        var result = await _sut.UpdateAsync(update);

        // Assert
        result.Version.Should().Be(2);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var plan = BuildValidPlan();
        plan.Id = "not-found";
        _repositoryMock
            .Setup(r => r.GetByIdAsync("not-found"))
            .ReturnsAsync((TrainingPlanModel?)null);

        // Act
        var act = async () => await _sut.UpdateAsync(plan);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_DeletesPlan()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.DeleteAsync("plan-001"))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteAsync("plan-001");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ApproveAsync_ValidPlan_SetsApprovedStatus()
    {
        // Arrange
        var plan = BuildValidPlan();
        plan.Id = "plan-001";
        plan.Status = TrainingPlanStatus.PendingApproval;
        var approved = BuildValidPlan();
        approved.Id = "plan-001";
        approved.Status = TrainingPlanStatus.Approved;
        _repositoryMock
            .Setup(r => r.GetByIdAsync("plan-001"))
            .ReturnsAsync(plan);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<TrainingPlanModel>()))
            .ReturnsAsync(approved);

        // Act
        var result = await _sut.ApproveAsync("plan-001", "manager-001");

        // Assert
        result.Status.Should().Be(TrainingPlanStatus.Approved);
    }

    [Fact]
    public async Task ApproveAsync_DraftPlan_ThrowsInvalidOperationException()
    {
        // Arrange
        var plan = BuildValidPlan();
        plan.Id = "plan-001";
        plan.Status = TrainingPlanStatus.Draft;
        _repositoryMock
            .Setup(r => r.GetByIdAsync("plan-001"))
            .ReturnsAsync(plan);

        // Act
        var act = async () => await _sut.ApproveAsync("plan-001", "manager-001");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetPagedAsync_ValidParams_ReturnsPagedResult()
    {
        // Arrange
        var pagedResult = new PagedResult<TrainingPlanModel>
        {
            Items = new List<TrainingPlanModel> { BuildValidPlan() },
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
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateAsync_BudgetBelowLegalMinimum_ThrowsInvalidOperationException()
    {
        // Arrange
        var plan = BuildValidPlan();
        plan.MasseSalariale = 1_000_000m;
        plan.LegalObligationRate = 0.01m;
        // Minimum required = 10,000 EUR, but we allocate only 5,000
        plan.BudgetAllocated = 5_000m;

        // Act
        var act = async () => await _sut.CreateAsync(plan);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateAsync_SetsDraftStatus_OnNewPlan()
    {
        // Arrange
        var plan = BuildValidPlan();
        TrainingPlanModel? captured = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TrainingPlanModel>()))
            .Callback<TrainingPlanModel>(p => captured = p)
            .ReturnsAsync((TrainingPlanModel p) => p);

        // Act
        await _sut.CreateAsync(plan);

        // Assert
        captured!.Status.Should().Be(TrainingPlanStatus.Draft);
    }

    private static TrainingPlanModel BuildValidPlan() =>
        new()
        {
            Title = "Plan de formation 2025",
            Year = 2025,
            BudgetAllocated = 50_000m,
            MasseSalariale = 0m,
            LegalObligationRate = 0.01m
        };
}
