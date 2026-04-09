using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Training.SkillDevelopment.DTOs.CertificationEcosysteme.Certification;
using Xunit;

namespace Training.SkillDevelopment.Tests.Integration.Certification;

public sealed class CertificationIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CertificationIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_ValidCertification_Returns201AndCertification()
    {
        // Arrange
        var dto = BuildValidCertificationDto();

        // Act
        var response = await _client.PostAsJsonAsync("/api/certifications", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CertificationRncpDto>();
        created.Should().NotBeNull();
        created!.RncpCode.Should().Be(dto.RncpCode);
    }

    [Fact]
    public async Task Create_MissingRncpCode_Returns400()
    {
        // Arrange
        var dto = BuildValidCertificationDto();
        dto.RncpCode = string.Empty;

        // Act
        var response = await _client.PostAsJsonAsync("/api/certifications", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_InvalidRncpCodeFormat_Returns400()
    {
        // Arrange
        var dto = BuildValidCertificationDto();
        dto.RncpCode = "INVALID-FORMAT-123";

        // Act
        var response = await _client.PostAsJsonAsync("/api/certifications", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_MissingTitle_Returns400()
    {
        // Arrange
        var dto = BuildValidCertificationDto();
        dto.Title = string.Empty;

        // Act
        var response = await _client.PostAsJsonAsync("/api/certifications", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_ExistingId_Returns200()
    {
        // Arrange
        var certificationId = await CreateCertificationAndGetId();

        // Act
        var response = await _client.GetAsync($"/api/certifications/{certificationId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CertificationRncpDto>();
        body.Should().NotBeNull();
        body!.Id.Should().Be(certificationId);
    }

    [Fact]
    public async Task GetById_NonExistentId_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/api/certifications/certification-does-not-exist");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_Returns200WithList()
    {
        // Arrange — ensure at least one certification exists
        await CreateCertificationAndGetId();

        // Act
        var response = await _client.GetAsync("/api/certifications");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var certifications = await response.Content.ReadFromJsonAsync<IReadOnlyList<CertificationRncpDto>>();
        certifications.Should().NotBeNull();
    }

    [Fact]
    public async Task GetExpiring_Returns200WithList()
    {
        // Act
        var response = await _client.GetAsync("/api/certifications/expiring?daysAhead=90");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var certifications = await response.Content.ReadFromJsonAsync<IReadOnlyList<CertificationRncpDto>>();
        certifications.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_ExistingCertification_Returns200WithUpdatedData()
    {
        // Arrange
        var certificationId = await CreateCertificationAndGetId();

        var updated = BuildValidCertificationDto();
        updated.Id = certificationId;
        updated.Title = "Technicien Superieur Systemes et Reseaux - Edition 2026";

        // Act
        var response = await _client.PutAsJsonAsync($"/api/certifications/{certificationId}", updated);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CertificationRncpDto>();
        body!.Title.Should().Be("Technicien Superieur Systemes et Reseaux - Edition 2026");
    }

    [Fact]
    public async Task Update_NonExistentId_Returns404()
    {
        // Arrange
        var dto = BuildValidCertificationDto();

        // Act
        var response = await _client.PutAsJsonAsync("/api/certifications/certification-does-not-exist", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ExistingId_Returns204()
    {
        // Arrange
        var certificationId = await CreateCertificationAndGetId();

        // Act
        var response = await _client.DeleteAsync($"/api/certifications/{certificationId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_NonExistentId_Returns204()
    {
        // The InMemory repository performs a silent TryRemove — no KeyNotFoundException is raised,
        // so the controller returns 204 regardless of whether the ID existed.
        var response = await _client.DeleteAsync("/api/certifications/certification-does-not-exist");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task<string> CreateCertificationAndGetId()
    {
        var dto = BuildValidCertificationDto();
        var response = await _client.PostAsJsonAsync("/api/certifications", dto);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CertificationRncpDto>();
        return created!.Id;
    }

    /// <summary>
    /// Builds a valid certification DTO with a unique RNCP code per call to prevent
    /// duplicate-RNCP-code conflicts in the shared InMemory store across tests.
    /// The RNCP regex requires digits only after the prefix: ^(RNCP|RS)\d+$
    /// </summary>
    private static CertificationRncpDto BuildValidCertificationDto()
    {
        var uniqueNumeric = Math.Abs(Guid.NewGuid().GetHashCode()) % 900000 + 10000;
        return new CertificationRncpDto
        {
            RncpCode = $"RNCP{uniqueNumeric}",
            Title = "BTS Services Informatiques aux Organisations option Solutions Logicielles et Applications Metiers",
            Description = "Certification reconnue par France Competences. Eligible CPF et OPCO.",
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
