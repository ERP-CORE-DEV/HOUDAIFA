using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Training.SkillDevelopment.DTOs.FormationExecution.TrainingPlan;
using Xunit;

namespace Training.SkillDevelopment.Tests.Integration.TrainingPlan;

public sealed class TrainingPlanIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TrainingPlanIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreatePlan_ValidData_Returns201AndPlan()
    {
        // Arrange
        var dto = BuildValidPlanDto();

        // Act
        var response = await _client.PostAsJsonAsync("/api/training-plans", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreatePlan_MissingTitle_Returns400()
    {
        // Arrange
        var dto = BuildValidPlanDto();
        dto.Title = string.Empty;

        // Act
        var response = await _client.PostAsJsonAsync("/api/training-plans", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePlan_NegativeBudget_Returns400()
    {
        // Arrange
        var dto = BuildValidPlanDto();
        dto.BudgetAllocated = -100m;

        // Act
        var response = await _client.PostAsJsonAsync("/api/training-plans", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPlanById_ExistingId_Returns200()
    {
        // Arrange
        var created = await CreatePlanAndGetId();

        // Act
        var response = await _client.GetAsync($"/api/training-plans/{created}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPlanById_NonExistentId_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/api/training-plans/non-existent-plan-id");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPlansPaged_ReturnsPagedResult()
    {
        // Arrange — ensure at least one plan exists
        await CreatePlanAndGetId();

        // Act
        var response = await _client.GetAsync("/api/training-plans?page=1&pageSize=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Items");
    }

    [Fact]
    public async Task UpdatePlan_ExistingPlan_Returns200()
    {
        // Arrange
        var planId = await CreatePlanAndGetId();
        var updateDto = BuildValidPlanDto();
        updateDto.Title = "Plan Mis a Jour 2026";

        // Act
        var response = await _client.PutAsJsonAsync($"/api/training-plans/{planId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeletePlan_ExistingId_Returns204()
    {
        // Arrange
        var planId = await CreatePlanAndGetId();

        // Act
        var response = await _client.DeleteAsync($"/api/training-plans/{planId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ApprovePlan_PlanInPendingApprovalStatus_Returns200()
    {
        // Arrange
        var planId = await CreatePlanAndGetId();

        // Move plan to PendingApproval state via update
        var updateDto = BuildValidPlanDto();
        updateDto.Status = "PendingApproval";
        await _client.PutAsJsonAsync($"/api/training-plans/{planId}", updateDto);

        var approveRequest = new { ApprovedBy = "manager-001" };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/training-plans/{planId}/approve", approveRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<string> CreatePlanAndGetId()
    {
        var dto = BuildValidPlanDto();
        var response = await _client.PostAsJsonAsync("/api/training-plans", dto);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<TrainingPlanDto>();
        return created!.Id;
    }

    private static TrainingPlanDto BuildValidPlanDto() =>
        new()
        {
            CompanyId = "company-test-001",
            Title = "Plan de Formation Integration 2026",
            Year = 2026,
            BudgetAllocated = 50_000m,
            MasseSalariale = 0m,
            LegalObligationRate = 0.01m,
            Status = "Draft"
        };
}
