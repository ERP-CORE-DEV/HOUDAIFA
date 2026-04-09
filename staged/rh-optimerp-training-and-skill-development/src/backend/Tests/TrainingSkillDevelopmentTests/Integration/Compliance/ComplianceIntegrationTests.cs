using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Training.SkillDevelopment.DTOs.FinancementConformite.Compliance;
using Training.SkillDevelopment.Models.Common;
using Xunit;

namespace Training.SkillDevelopment.Tests.Integration.Compliance;

public sealed class ComplianceIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ComplianceIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_ValidObligation_Returns201AndObligation()
    {
        // Arrange
        var dto = BuildValidObligationDto();

        // Act
        var response = await _client.PostAsJsonAsync("/api/training-obligations", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<TrainingObligationDto>(TestJsonOptions.Default);
        created.Should().NotBeNull();
        created!.Title.Should().Be(dto.Title);
    }

    [Fact]
    public async Task Create_MissingTitle_Returns400()
    {
        // Arrange
        var dto = BuildValidObligationDto();
        dto.Title = string.Empty;

        // Act
        var response = await _client.PostAsJsonAsync("/api/training-obligations", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_FrequencyOutOfRange_Returns400()
    {
        // Arrange
        var dto = BuildValidObligationDto();
        dto.FrequencyMonths = 0;

        // Act
        var response = await _client.PostAsJsonAsync("/api/training-obligations", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_ExistingId_Returns200()
    {
        // Arrange
        var obligationId = await CreateObligationAndGetId();

        // Act
        var response = await _client.GetAsync($"/api/training-obligations/{obligationId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TrainingObligationDto>(TestJsonOptions.Default);
        body.Should().NotBeNull();
        body!.Id.Should().Be(obligationId);
    }

    [Fact]
    public async Task GetById_NonExistentId_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/api/training-obligations/obligation-does-not-exist");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_Returns200WithList()
    {
        // Arrange — ensure at least one obligation exists
        await CreateObligationAndGetId();

        // Act
        var response = await _client.GetAsync("/api/training-obligations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var obligations = await response.Content.ReadFromJsonAsync<IReadOnlyList<TrainingObligationDto>>(TestJsonOptions.Default);
        obligations.Should().NotBeNull();
    }

    [Fact]
    public async Task GetExpiring_Returns200WithList()
    {
        // Act
        var response = await _client.GetAsync("/api/training-obligations/expiring?daysAhead=90");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var obligations = await response.Content.ReadFromJsonAsync<IReadOnlyList<TrainingObligationDto>>(TestJsonOptions.Default);
        obligations.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRiskAssessment_Returns200WithAlerts()
    {
        // Act
        var response = await _client.GetAsync("/api/training-obligations/risk-assessment");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Update_ExistingObligation_Returns200WithUpdatedData()
    {
        // Arrange
        var obligationId = await CreateObligationAndGetId();
        var updated = BuildValidObligationDto();
        updated.Id = obligationId;
        updated.Title = "Formation Habilitation Electrique B1V - Revision 2026";
        updated.FrequencyMonths = 36;
        updated.RiskLevel = RiskLevel.High;

        // Act
        var response = await _client.PutAsJsonAsync($"/api/training-obligations/{obligationId}", updated);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TrainingObligationDto>(TestJsonOptions.Default);
        body!.Title.Should().Be("Formation Habilitation Electrique B1V - Revision 2026");
        body.FrequencyMonths.Should().Be(36);
    }

    [Fact]
    public async Task Update_NonExistentId_Returns404()
    {
        // Arrange
        var dto = BuildValidObligationDto();

        // Act
        var response = await _client.PutAsJsonAsync("/api/training-obligations/obligation-does-not-exist", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<string> CreateObligationAndGetId()
    {
        var dto = BuildValidObligationDto();
        var response = await _client.PostAsJsonAsync("/api/training-obligations", dto);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<TrainingObligationDto>(TestJsonOptions.Default);
        return created!.Id;
    }

    private static TrainingObligationDto BuildValidObligationDto() =>
        new()
        {
            Title = "Formation Habilitation Electrique B1V",
            Description = "Obligation reglementaire issue du decret n 2010-1118 du 22 septembre 2010. Concerne les travailleurs exposes aux risques electriques.",
            RegulatoryReference = "Decret 2010-1118 - NFC 18-510",
            TargetJobFamily = "Electriciens et Techniciens de Maintenance",
            FrequencyMonths = 12,
            RiskLevel = RiskLevel.Critical,
            Status = "Current",
            EffectiveDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ExpirationDate = new DateTime(2027, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            IsActive = true
        };
}
