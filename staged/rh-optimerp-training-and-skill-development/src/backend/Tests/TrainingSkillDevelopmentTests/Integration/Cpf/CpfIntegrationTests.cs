using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Training.SkillDevelopment.DTOs.FinancementConformite.Cpf;
using Xunit;

namespace Training.SkillDevelopment.Tests.Integration.Cpf;

public sealed class CpfIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CpfIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAccountByEmployeeId_ExistingEmployee_Returns200()
    {
        // Arrange
        var employeeId = $"emp-{Guid.NewGuid()}";
        await CreateAccountForEmployee(employeeId);

        // Act
        var response = await _client.GetAsync($"/api/cpf/by-employee/{employeeId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAccountByEmployeeId_NonExistentEmployee_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/api/cpf/by-employee/employee-does-not-exist");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAccount_ValidData_Returns201()
    {
        // Arrange
        var dto = BuildValidAccountDto($"emp-{Guid.NewGuid()}");

        // Act
        var response = await _client.PostAsJsonAsync("/api/cpf", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateAccount_MissingEmployeeId_Returns400()
    {
        // Arrange
        var dto = BuildValidAccountDto(string.Empty);

        // Act
        var response = await _client.PostAsJsonAsync("/api/cpf", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ApplyAnnualCredit_StandardEmployee_Credits500Euros()
    {
        // Arrange
        var employeeId = $"emp-{Guid.NewGuid()}";
        var accountId = await CreateAccountForEmployee(employeeId, isLowQualified: false);

        // Act
        var response = await _client.PostAsync($"/api/cpf/{accountId}/credit-annual", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var account = await response.Content.ReadFromJsonAsync<CpfAccountDto>();
        account!.BalanceEuros.Should().Be(500m);
    }

    [Fact]
    public async Task ApplyAnnualCredit_LowQualifiedEmployee_Credits800Euros()
    {
        // Arrange
        var employeeId = $"emp-{Guid.NewGuid()}";
        var accountId = await CreateAccountForEmployee(employeeId, isLowQualified: true);

        // Act
        var response = await _client.PostAsync($"/api/cpf/{accountId}/credit-annual", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var account = await response.Content.ReadFromJsonAsync<CpfAccountDto>();
        account!.BalanceEuros.Should().Be(800m);
    }

    [Fact]
    public async Task ApplyAnnualCredit_NonExistentAccount_Returns404()
    {
        // Act
        var response = await _client.PostAsync("/api/cpf/account-does-not-exist/credit-annual", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<string> CreateAccountForEmployee(string employeeId, bool isLowQualified = false)
    {
        var dto = BuildValidAccountDto(employeeId, isLowQualified);
        var response = await _client.PostAsJsonAsync("/api/cpf", dto);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CpfAccountDto>();
        return created!.Id;
    }

    private static CpfAccountDto BuildValidAccountDto(string employeeId, bool isLowQualified = false) =>
        new()
        {
            EmployeeId = employeeId,
            IsLowQualified = isLowQualified,
            IsActive = true
        };
}
