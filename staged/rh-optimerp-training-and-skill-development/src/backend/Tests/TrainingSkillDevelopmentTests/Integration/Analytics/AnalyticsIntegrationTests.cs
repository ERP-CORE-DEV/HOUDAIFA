using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Training.SkillDevelopment.DTOs.PilotageGouvernance.Analytics;
using Xunit;

namespace Training.SkillDevelopment.Tests.Integration.Analytics;

public sealed class AnalyticsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AnalyticsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDashboardKpis_ValidYear_Returns200()
    {
        // Act
        var response = await _client.GetAsync("/api/training-analytics/dashboard-kpis?year=2026");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var kpis = await response.Content.ReadFromJsonAsync<TrainingDashboardKpiDto>();
        kpis.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDashboardKpis_NoYearParam_Returns200WithCurrentYear()
    {
        // Act
        var response = await _client.GetAsync("/api/training-analytics/dashboard-kpis");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetBilanSocial_ValidYear_Returns200()
    {
        // Act
        var response = await _client.GetAsync("/api/training-analytics/bilan-social?year=2026");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var bilan = await response.Content.ReadFromJsonAsync<BilanSocialTrainingDto>();
        bilan.Should().NotBeNull();
    }

    [Fact]
    public async Task GetGenderEqualityReport_ValidYear_Returns200()
    {
        // Act
        var response = await _client.GetAsync("/api/training-analytics/gender-equality-report?year=2026");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<GenderEqualityTrainingReportDto>();
        report.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTrends_ValidRange_Returns200WithList()
    {
        // Act
        var response = await _client.GetAsync("/api/training-analytics/trends?startYear=2023&endYear=2026");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var trends = await response.Content.ReadFromJsonAsync<IReadOnlyList<TrainingTrendDto>>();
        trends.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTrends_NoParams_Returns200WithDefaultRange()
    {
        // Act
        var response = await _client.GetAsync("/api/training-analytics/trends");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
