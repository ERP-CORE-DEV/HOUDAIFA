using System.Diagnostics;
using System.Net.Http.Json;
using FluentAssertions;
using Training.SkillDevelopment.DTOs.CertificationEcosysteme.Certification;
using Training.SkillDevelopment.DTOs.Competency;
using Training.SkillDevelopment.DTOs.FormationExecution.TrainingPlan;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Tests.Integration;
using Xunit;

namespace Training.SkillDevelopment.Tests.Performance;

/// <summary>
/// Tests verifying that HTTP API endpoints respond within the performance SLAs:
/// single-resource ops &lt; 100ms, list/paginated ops &lt; 200ms, health check &lt; 50ms.
/// Uses the shared <see cref="CustomWebApplicationFactory"/> with InMemory repositories
/// so no external infrastructure is required.
/// </summary>
public sealed class ApiPerformanceTests : IClassFixture<CustomWebApplicationFactory>
{
    private const int SingleEndpointLimitMs = 100;
    private const int ListEndpointLimitMs = 200;
    private const int HealthCheckLimitMs = 50;

    private readonly HttpClient _client;

    public ApiPerformanceTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // -------------------------------------------------------------------------
    // Health check
    // -------------------------------------------------------------------------

    [Fact]
    public async Task HealthCheck_LiveEndpoint_ShouldRespondWithin50ms()
    {
        // Arrange — warm up JIT on first call, then measure
        await _client.GetAsync("/health/live");
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/health/live");

        // Assert
        stopwatch.Stop();
        response.IsSuccessStatusCode.Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(HealthCheckLimitMs));
    }

    [Fact]
    public async Task HealthCheck_ReadyEndpoint_ShouldRespondWithin50ms()
    {
        // Arrange — warm up
        await _client.GetAsync("/health/ready");
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        stopwatch.Stop();
        response.IsSuccessStatusCode.Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(HealthCheckLimitMs));
    }

    // -------------------------------------------------------------------------
    // Training Plan endpoints
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PostTrainingPlan_CreateSingleResource_ShouldRespondWithin100ms()
    {
        // Arrange — warm up the pipeline
        await _client.PostAsJsonAsync("/api/training-plans", BuildTrainingPlanDto());
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.PostAsJsonAsync("/api/training-plans", BuildTrainingPlanDto());

        // Assert
        stopwatch.Stop();
        response.IsSuccessStatusCode.Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SingleEndpointLimitMs));
    }

    [Fact]
    public async Task GetTrainingPlanById_SingleResource_ShouldRespondWithin100ms()
    {
        // Arrange
        var planId = await CreateTrainingPlanAndGetId();
        // Warm up
        await _client.GetAsync($"/api/training-plans/{planId}");
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync($"/api/training-plans/{planId}");

        // Assert
        stopwatch.Stop();
        response.IsSuccessStatusCode.Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SingleEndpointLimitMs));
    }

    [Fact]
    public async Task GetTrainingPlansPaged_ListResource_ShouldRespondWithin200ms()
    {
        // Arrange — seed data and warm up
        await CreateTrainingPlanAndGetId();
        await _client.GetAsync("/api/training-plans?page=1&pageSize=20");
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/api/training-plans?page=1&pageSize=20");

        // Assert
        stopwatch.Stop();
        response.IsSuccessStatusCode.Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(ListEndpointLimitMs));
    }

    [Fact]
    public async Task PutTrainingPlan_UpdateSingleResource_ShouldRespondWithin100ms()
    {
        // Arrange
        var planId = await CreateTrainingPlanAndGetId();
        var updateDto = BuildTrainingPlanDto();
        updateDto.Title = "Plan Mis a Jour Performance API 2026";
        // Warm up
        await _client.PutAsJsonAsync($"/api/training-plans/{planId}", updateDto);
        // Re-create for a fresh update
        var freshId = await CreateTrainingPlanAndGetId();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/training-plans/{freshId}", updateDto);

        // Assert
        stopwatch.Stop();
        response.IsSuccessStatusCode.Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SingleEndpointLimitMs));
    }

    [Fact]
    public async Task DeleteTrainingPlan_SingleResource_ShouldRespondWithin100ms()
    {
        // Arrange — create two plans: one for warm-up, one for measurement
        var warmUpId = await CreateTrainingPlanAndGetId();
        await _client.DeleteAsync($"/api/training-plans/{warmUpId}");
        var planId = await CreateTrainingPlanAndGetId();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.DeleteAsync($"/api/training-plans/{planId}");

        // Assert
        stopwatch.Stop();
        response.IsSuccessStatusCode.Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SingleEndpointLimitMs));
    }

    // -------------------------------------------------------------------------
    // Competency endpoints
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PostCompetency_CreateSingleResource_ShouldRespondWithin100ms()
    {
        // Arrange — warm up
        await _client.PostAsJsonAsync("/api/competencies", BuildCompetencyDto());
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.PostAsJsonAsync("/api/competencies", BuildCompetencyDto());

        // Assert
        stopwatch.Stop();
        response.IsSuccessStatusCode.Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SingleEndpointLimitMs));
    }

    [Fact]
    public async Task GetCompetencyById_SingleResource_ShouldRespondWithin100ms()
    {
        // Arrange
        var competencyId = await CreateCompetencyAndGetId();
        // Warm up
        await _client.GetAsync($"/api/competencies/{competencyId}");
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync($"/api/competencies/{competencyId}");

        // Assert
        stopwatch.Stop();
        response.IsSuccessStatusCode.Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SingleEndpointLimitMs));
    }

    [Fact]
    public async Task GetCompetencies_ListResource_ShouldRespondWithin200ms()
    {
        // Arrange — seed data and warm up
        await CreateCompetencyAndGetId();
        await _client.GetAsync("/api/competencies");
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/api/competencies");

        // Assert
        stopwatch.Stop();
        response.IsSuccessStatusCode.Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(ListEndpointLimitMs));
    }

    [Fact]
    public async Task PutCompetency_UpdateSingleResource_ShouldRespondWithin100ms()
    {
        // Arrange — warm-up call then fresh resource
        var warmUpId = await CreateCompetencyAndGetId();
        var updateDto = BuildCompetencyDto();
        await _client.PutAsJsonAsync($"/api/competencies/{warmUpId}", updateDto);
        var freshId = await CreateCompetencyAndGetId();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/competencies/{freshId}", updateDto);

        // Assert
        stopwatch.Stop();
        response.IsSuccessStatusCode.Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SingleEndpointLimitMs));
    }

    [Fact]
    public async Task DeleteCompetency_SingleResource_ShouldRespondWithin100ms()
    {
        // Arrange
        var warmUpId = await CreateCompetencyAndGetId();
        await _client.DeleteAsync($"/api/competencies/{warmUpId}");
        var competencyId = await CreateCompetencyAndGetId();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.DeleteAsync($"/api/competencies/{competencyId}");

        // Assert
        stopwatch.Stop();
        response.IsSuccessStatusCode.Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SingleEndpointLimitMs));
    }

    // -------------------------------------------------------------------------
    // Certification endpoints
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PostCertification_CreateSingleResource_ShouldRespondWithin100ms()
    {
        // Arrange — warm up
        await _client.PostAsJsonAsync("/api/certifications", BuildCertificationDto());
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.PostAsJsonAsync("/api/certifications", BuildCertificationDto());

        // Assert
        stopwatch.Stop();
        response.IsSuccessStatusCode.Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SingleEndpointLimitMs));
    }

    [Fact]
    public async Task GetCertificationById_SingleResource_ShouldRespondWithin100ms()
    {
        // Arrange
        var certificationId = await CreateCertificationAndGetId();
        // Warm up
        await _client.GetAsync($"/api/certifications/{certificationId}");
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync($"/api/certifications/{certificationId}");

        // Assert
        stopwatch.Stop();
        response.IsSuccessStatusCode.Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SingleEndpointLimitMs));
    }

    [Fact]
    public async Task GetCertifications_ListResource_ShouldRespondWithin200ms()
    {
        // Arrange — seed data and warm up
        await CreateCertificationAndGetId();
        await _client.GetAsync("/api/certifications");
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/api/certifications");

        // Assert
        stopwatch.Stop();
        response.IsSuccessStatusCode.Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(ListEndpointLimitMs));
    }

    [Fact]
    public async Task DeleteCertification_SingleResource_ShouldRespondWithin100ms()
    {
        // Arrange
        var warmUpId = await CreateCertificationAndGetId();
        await _client.DeleteAsync($"/api/certifications/{warmUpId}");
        var certificationId = await CreateCertificationAndGetId();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.DeleteAsync($"/api/certifications/{certificationId}");

        // Assert
        stopwatch.Stop();
        response.IsSuccessStatusCode.Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SingleEndpointLimitMs));
    }

    // -------------------------------------------------------------------------
    // Helper methods
    // -------------------------------------------------------------------------

    private async Task<string> CreateTrainingPlanAndGetId()
    {
        var response = await _client.PostAsJsonAsync("/api/training-plans", BuildTrainingPlanDto());
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<TrainingPlanDto>();
        return created!.Id;
    }

    private async Task<string> CreateCompetencyAndGetId()
    {
        var response = await _client.PostAsJsonAsync("/api/competencies", BuildCompetencyDto());
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CompetencyDto>(TestJsonOptions.Default);
        return created!.Id;
    }

    private async Task<string> CreateCertificationAndGetId()
    {
        var response = await _client.PostAsJsonAsync("/api/certifications", BuildCertificationDto());
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CertificationRncpDto>();
        return created!.Id;
    }

    private static TrainingPlanDto BuildTrainingPlanDto() =>
        new()
        {
            CompanyId = "company-perf-api-001",
            Title = "Plan de Formation API Performance 2026",
            Year = 2026,
            BudgetAllocated = 50_000m,
            MasseSalariale = 0m,
            LegalObligationRate = 0.01m,
            Status = "Draft"
        };

    private static CompetencyDto BuildCompetencyDto() =>
        new()
        {
            Code = $"COMP-{Guid.NewGuid():N}"[..12],
            Name = "Gestion de la securite au travail - Performance",
            Description = "Competence generee pour les tests de performance API.",
            Domain = "Securite au travail",
            Family = "Prevention des risques",
            Type = CompetencyType.Technical,
            IsCritical = false,
            IsActive = true,
            Version = 1
        };

    private static CertificationRncpDto BuildCertificationDto()
    {
        var uniqueNumeric = Math.Abs(Guid.NewGuid().GetHashCode()) % 900_000 + 10_000;
        return new CertificationRncpDto
        {
            RncpCode = $"RNCP{uniqueNumeric}",
            Title = "BTS SIO option SLAM - Test de Performance API",
            Description = "Certification generee pour les tests de performance API.",
            CertifyingBody = "Ministere de l'Education Nationale",
            NsfCode = 326,
            Level = "5",
            RegistrationDate = new DateTime(2021, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            ExpirationDate = new DateTime(2026, 8, 31, 0, 0, 0, DateTimeKind.Utc),
            EligibleOpcoIds = ["OPCO-EP", "ATLAS"],
            IsActive = true
        };
    }
}
