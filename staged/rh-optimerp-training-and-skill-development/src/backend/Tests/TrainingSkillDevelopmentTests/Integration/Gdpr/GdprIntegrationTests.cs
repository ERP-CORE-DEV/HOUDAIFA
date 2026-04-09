using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Training.SkillDevelopment.DTOs.PilotageGouvernance.Gdpr;
using Xunit;

namespace Training.SkillDevelopment.Tests.Integration.Gdpr;

public sealed class GdprIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GdprIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AnonymizeEmployee_ValidId_Returns204()
    {
        // Arrange
        var employeeId = $"emp-{Guid.NewGuid()}";

        // Act
        var response = await _client.PostAsync($"/api/gdpr/anonymize/{employeeId}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ExportEmployeeData_ValidId_Returns200WithExport()
    {
        // Arrange
        var employeeId = $"emp-{Guid.NewGuid()}";

        // Act
        var response = await _client.GetAsync($"/api/gdpr/export/{employeeId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var export = await response.Content.ReadFromJsonAsync<EmployeeTrainingDataExportDto>();
        export.Should().NotBeNull();
        export!.EmployeeId.Should().Be(employeeId);
    }

    [Fact]
    public async Task GetRetentionStatus_Returns200WithReport()
    {
        // Act
        var response = await _client.GetAsync("/api/gdpr/retention-status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<DataRetentionReportDto>();
        report.Should().NotBeNull();
        report!.TotalRecordsAnalyzed.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task AnonymizeEmployee_EmptyId_Returns404()
    {
        // Act — an empty segment resolves to a different route, not a blank employeeId
        var response = await _client.PostAsync("/api/gdpr/anonymize/%20", null);

        // Assert — whitespace-only ID should fail at the service layer (400) or route mismatch (404)
        ((int)response.StatusCode).Should().BeOneOf(400, 404);
    }

    [Fact]
    public async Task ExportEmployeeData_EmptyId_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/api/gdpr/export/%20");

        // Assert
        ((int)response.StatusCode).Should().BeOneOf(400, 404);
    }
}
