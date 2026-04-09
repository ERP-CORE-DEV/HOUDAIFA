using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Training.SkillDevelopment.DTOs.FinancementConformite.Alternance;
using Xunit;

namespace Training.SkillDevelopment.Tests.Integration.Alternance;

public sealed class AlternanceIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AlternanceIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_ValidApprentissageContract_Returns201AndContract()
    {
        // Arrange
        var dto = BuildValidContractDto($"emp-{Guid.NewGuid()}");

        // Act
        var response = await _client.PostAsJsonAsync("/api/alternance-contracts", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<AlternanceContractDto>();
        created.Should().NotBeNull();
        created!.EmployeeId.Should().Be(dto.EmployeeId);
    }

    [Fact]
    public async Task Create_MissingEmployeeId_Returns400()
    {
        // Arrange
        var dto = BuildValidContractDto(string.Empty);

        // Act
        var response = await _client.PostAsJsonAsync("/api/alternance-contracts", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_InvalidRemunerationPercentage_Returns400()
    {
        // Arrange
        var dto = BuildValidContractDto($"emp-{Guid.NewGuid()}");
        dto.RemunerationPercentage = 150m;

        // Act
        var response = await _client.PostAsJsonAsync("/api/alternance-contracts", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_ExistingId_Returns200()
    {
        // Arrange
        var contractId = await CreateContractAndGetId();

        // Act
        var response = await _client.GetAsync($"/api/alternance-contracts/{contractId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AlternanceContractDto>();
        body.Should().NotBeNull();
        body!.Id.Should().Be(contractId);
    }

    [Fact]
    public async Task GetById_NonExistentId_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/api/alternance-contracts/contract-does-not-exist");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByEmployeeId_ExistingEmployee_Returns200WithContracts()
    {
        // Arrange
        var employeeId = $"emp-{Guid.NewGuid()}";
        await CreateContractAndGetId(employeeId);

        // Act
        var response = await _client.GetAsync($"/api/alternance-contracts/by-employee/{employeeId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var contracts = await response.Content.ReadFromJsonAsync<IReadOnlyList<AlternanceContractDto>>();
        contracts.Should().NotBeNull();
        contracts!.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetByEmployeeId_UnknownEmployee_Returns200WithEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/alternance-contracts/by-employee/employee-no-contracts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var contracts = await response.Content.ReadFromJsonAsync<IReadOnlyList<AlternanceContractDto>>();
        contracts.Should().NotBeNull();
        contracts!.Should().BeEmpty();
    }

    [Fact]
    public async Task CalculateRemuneration_ExistingContract_Returns200WithAmount()
    {
        // Arrange
        var contractId = await CreateContractAndGetId();

        // Act
        var response = await _client.GetAsync($"/api/alternance-contracts/calculate-remuneration?contractId={contractId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("remunerationAmount");
    }

    [Fact]
    public async Task CalculateRemuneration_MissingContractId_Returns400()
    {
        // Act
        var response = await _client.GetAsync("/api/alternance-contracts/calculate-remuneration?contractId=");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_ExistingContract_Returns200WithUpdatedData()
    {
        // Arrange
        var contractId = await CreateContractAndGetId();
        var updated = BuildValidContractDto($"emp-{Guid.NewGuid()}");
        updated.Id = contractId;
        updated.RemunerationPercentage = 65m;
        updated.TutorId = "tutor-rh-senior-001";

        // Act
        var response = await _client.PutAsJsonAsync($"/api/alternance-contracts/{contractId}", updated);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AlternanceContractDto>();
        body!.RemunerationPercentage.Should().Be(65m);
    }

    [Fact]
    public async Task Update_NonExistentId_Returns404()
    {
        // Arrange
        var dto = BuildValidContractDto($"emp-{Guid.NewGuid()}");

        // Act
        var response = await _client.PutAsJsonAsync("/api/alternance-contracts/contract-does-not-exist", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ExistingId_Returns204()
    {
        // Arrange
        var contractId = await CreateContractAndGetId();

        // Act
        var response = await _client.DeleteAsync($"/api/alternance-contracts/{contractId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_NonExistentId_Returns204()
    {
        // The InMemory repository performs a silent TryRemove — no KeyNotFoundException is raised,
        // so the controller returns 204 regardless of whether the ID existed.
        var response = await _client.DeleteAsync("/api/alternance-contracts/contract-does-not-exist");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task<string> CreateContractAndGetId(string? employeeId = null)
    {
        var dto = BuildValidContractDto(employeeId ?? $"emp-{Guid.NewGuid()}");
        var response = await _client.PostAsJsonAsync("/api/alternance-contracts", dto);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<AlternanceContractDto>();
        return created!.Id;
    }

    private static AlternanceContractDto BuildValidContractDto(string employeeId) =>
        new()
        {
            EmployeeId = employeeId,
            Type = "Apprentissage",
            StartDate = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2028, 8, 31, 0, 0, 0, DateTimeKind.Utc),
            CfaId = "CFA-IDF-001",
            TutorId = "tutor-martin-dupont",
            CertificationTargetId = "cert-rncp35975",
            RemunerationPercentage = 53m,
            Status = "Active"
        };
}
