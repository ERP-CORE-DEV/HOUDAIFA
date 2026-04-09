using System.Diagnostics;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Training.SkillDevelopment.DTOs.FormationExecution.TrainingPlan;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.Competency;
using Training.SkillDevelopment.Repositories.Competency;
using Training.SkillDevelopment.Repositories.FormationExecution.TrainingPlan;
using Training.SkillDevelopment.Tests.Integration;
using Xunit;

namespace Training.SkillDevelopment.Tests.Performance;

/// <summary>
/// Tests verifying that concurrent operations on InMemory repositories and API
/// endpoints complete within acceptable elapsed-wall-clock SLAs:
/// 10 concurrent reads &lt; 500ms, 5 concurrent writes &lt; 1s.
/// </summary>
public sealed class ConcurrencyTests : IClassFixture<CustomWebApplicationFactory>
{
    private const int ConcurrentReadLimitMs = 500;
    private const int ConcurrentWriteLimitMs = 1000;
    private const int MixedWorkloadLimitMs = 1500;

    private readonly HttpClient _client;

    public ConcurrencyTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // -------------------------------------------------------------------------
    // Repository-level concurrency (InMemory ConcurrentDictionary)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Repository_10ConcurrentReads_ShouldCompleteWithin500ms()
    {
        // Arrange
        var repository = new InMemoryCompetencyRepository();
        var seeded = await SeedCompetenciesAsync(repository, count: 10);

        var stopwatch = Stopwatch.StartNew();

        // Act
        var readTasks = seeded.Select(c => repository.GetByIdAsync(c.Id)).ToArray();
        await Task.WhenAll(readTasks);

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(ConcurrentReadLimitMs));
        readTasks.All(t => t.Result is not null).Should().BeTrue();
    }

    [Fact]
    public async Task Repository_5ConcurrentWrites_ShouldCompleteWithin1s()
    {
        // Arrange
        var repository = new InMemoryCompetencyRepository();
        var competencies = Enumerable.Range(0, 5).Select(_ => BuildCompetency()).ToList();

        var stopwatch = Stopwatch.StartNew();

        // Act
        var writeTasks = competencies.Select(c => repository.CreateAsync(c)).ToArray();
        await Task.WhenAll(writeTasks);

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(ConcurrentWriteLimitMs));
        writeTasks.All(t => t.IsCompletedSuccessfully).Should().BeTrue();
    }

    [Fact]
    public async Task Repository_MixedReadWriteWorkload_ShouldCompleteWithin1500ms()
    {
        // Arrange
        var repository = new InMemoryCompetencyRepository();
        var seeded = await SeedCompetenciesAsync(repository, count: 5);

        var readTasks = seeded.Select(c => repository.GetByIdAsync(c.Id));
        var writeCompetencies = Enumerable.Range(0, 5).Select(_ => BuildCompetency()).ToList();
        var writeTasks = writeCompetencies.Select(c => repository.CreateAsync(c));
        var updateTasks = seeded.Take(3).Select(c =>
        {
            c.Name = $"Competence Mise a Jour Concurrence {Guid.NewGuid():N}"[..45];
            return repository.UpdateAsync(c);
        });

        var allTasks = readTasks
            .Cast<Task>()
            .Concat(writeTasks.Cast<Task>())
            .Concat(updateTasks.Cast<Task>())
            .ToArray();

        var stopwatch = Stopwatch.StartNew();

        // Act
        await Task.WhenAll(allTasks);

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(MixedWorkloadLimitMs));
        allTasks.All(t => t.IsCompletedSuccessfully).Should().BeTrue();
    }

    [Fact]
    public async Task Repository_10ConcurrentTrainingPlanReads_ShouldCompleteWithin500ms()
    {
        // Arrange
        var repository = new InMemoryTrainingPlanRepository(NullLogger<InMemoryTrainingPlanRepository>.Instance);
        var plans = new List<Models.FormationExecution.TrainingPlan.TrainingPlan>();
        for (var i = 0; i < 10; i++)
        {
            plans.Add(await repository.AddAsync(BuildTrainingPlan()));
        }

        var stopwatch = Stopwatch.StartNew();

        // Act
        var readTasks = plans.Select(p => repository.GetByIdAsync(p.Id)).ToArray();
        await Task.WhenAll(readTasks);

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(ConcurrentReadLimitMs));
        readTasks.All(t => t.Result is not null).Should().BeTrue();
    }

    [Fact]
    public async Task Repository_5ConcurrentTrainingPlanWrites_ShouldCompleteWithin1s()
    {
        // Arrange
        var repository = new InMemoryTrainingPlanRepository(NullLogger<InMemoryTrainingPlanRepository>.Instance);
        var plans = Enumerable.Range(0, 5).Select(_ => BuildTrainingPlan()).ToList();

        var stopwatch = Stopwatch.StartNew();

        // Act
        var writeTasks = plans.Select(p => repository.AddAsync(p)).ToArray();
        await Task.WhenAll(writeTasks);

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(ConcurrentWriteLimitMs));
        writeTasks.All(t => t.IsCompletedSuccessfully).Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // API-level concurrency (HttpClient against WebApplicationFactory)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Api_10ConcurrentGetRequests_ShouldCompleteWithin500ms()
    {
        // Arrange — seed a resource then warm up the pipeline
        var planId = await CreateTrainingPlanViaApiAndGetId();
        await _client.GetAsync($"/api/training-plans/{planId}");

        var stopwatch = Stopwatch.StartNew();

        // Act
        var requests = Enumerable.Range(0, 10)
            .Select(_ => _client.GetAsync($"/api/training-plans/{planId}"))
            .ToArray();
        var responses = await Task.WhenAll(requests);

        // Assert
        stopwatch.Stop();
        responses.All(r => r.IsSuccessStatusCode).Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(ConcurrentReadLimitMs));
    }

    [Fact]
    public async Task Api_5ConcurrentPostRequests_ShouldCompleteWithin1s()
    {
        // Arrange — warm up the pipeline first
        await _client.PostAsJsonAsync("/api/training-plans", BuildTrainingPlanDto());

        var stopwatch = Stopwatch.StartNew();

        // Act
        var requests = Enumerable.Range(0, 5)
            .Select(_ => _client.PostAsJsonAsync("/api/training-plans", BuildTrainingPlanDto()))
            .ToArray();
        var responses = await Task.WhenAll(requests);

        // Assert
        stopwatch.Stop();
        responses.All(r => r.IsSuccessStatusCode).Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(ConcurrentWriteLimitMs));
    }

    [Fact]
    public async Task Api_MixedConcurrentReadWriteWorkload_ShouldCompleteWithin1500ms()
    {
        // Arrange — seed resources and warm up
        var id1 = await CreateTrainingPlanViaApiAndGetId();
        var id2 = await CreateTrainingPlanViaApiAndGetId();
        await _client.GetAsync($"/api/training-plans/{id1}");

        var stopwatch = Stopwatch.StartNew();

        // Act — mix reads and writes in parallel
        var reads = new[]
        {
            _client.GetAsync($"/api/training-plans/{id1}"),
            _client.GetAsync($"/api/training-plans/{id2}"),
            _client.GetAsync("/api/training-plans?page=1&pageSize=20"),
            _client.GetAsync($"/api/training-plans/{id1}"),
            _client.GetAsync("/health/live"),
        };

        var writes = new[]
        {
            _client.PostAsJsonAsync("/api/training-plans", BuildTrainingPlanDto()),
            _client.PostAsJsonAsync("/api/training-plans", BuildTrainingPlanDto()),
            _client.PostAsJsonAsync("/api/training-plans", BuildTrainingPlanDto()),
        };

        var allRequests = reads.Cast<Task>().Concat(writes.Cast<Task>()).ToArray();
        await Task.WhenAll(allRequests);

        // Assert
        stopwatch.Stop();
        allRequests.All(t => t.IsCompletedSuccessfully).Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(MixedWorkloadLimitMs));
    }

    [Fact]
    public async Task Api_10ConcurrentHealthChecks_ShouldCompleteWithin500ms()
    {
        // Arrange — warm up
        await _client.GetAsync("/health/live");

        var stopwatch = Stopwatch.StartNew();

        // Act
        var requests = Enumerable.Range(0, 10)
            .Select(_ => _client.GetAsync("/health/live"))
            .ToArray();
        var responses = await Task.WhenAll(requests);

        // Assert
        stopwatch.Stop();
        responses.All(r => r.IsSuccessStatusCode).Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(ConcurrentReadLimitMs));
    }

    // -------------------------------------------------------------------------
    // Helper methods
    // -------------------------------------------------------------------------

    private async Task<string> CreateTrainingPlanViaApiAndGetId()
    {
        var response = await _client.PostAsJsonAsync("/api/training-plans", BuildTrainingPlanDto());
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<TrainingPlanDto>();
        return created!.Id;
    }

    private static async Task<IReadOnlyList<Competency>> SeedCompetenciesAsync(
        InMemoryCompetencyRepository repository,
        int count)
    {
        var result = new List<Competency>();
        for (var i = 0; i < count; i++)
        {
            result.Add(await repository.CreateAsync(BuildCompetency()));
        }

        return result.AsReadOnly();
    }

    private static Competency BuildCompetency() =>
        new()
        {
            Id = Guid.NewGuid().ToString(),
            Code = $"COMP-CONC-{Guid.NewGuid():N}"[..18],
            Name = $"Competence Concurrence {Guid.NewGuid():N}"[..30],
            Description = "Competence generee pour les tests de concurrence.",
            Domain = "Tests Concurrence",
            Family = "Performance",
            Type = CompetencyType.Technical,
            IsCritical = false,
            IsActive = true,
            Version = 1
        };

    private static Models.FormationExecution.TrainingPlan.TrainingPlan BuildTrainingPlan() =>
        new()
        {
            Id = Guid.NewGuid().ToString(),
            CompanyId = "company-conc-001",
            Title = $"Plan Concurrence {Guid.NewGuid():N}"[..35],
            Year = 2026,
            BudgetAllocated = 30_000m,
            MasseSalariale = 0m,
            LegalObligationRate = 0.01m,
            Status = TrainingPlanStatus.Draft,
            IsActive = true
        };

    private static TrainingPlanDto BuildTrainingPlanDto() =>
        new()
        {
            CompanyId = "company-conc-api-001",
            Title = "Plan de Formation Concurrence API 2026",
            Year = 2026,
            BudgetAllocated = 30_000m,
            MasseSalariale = 0m,
            LegalObligationRate = 0.01m,
            Status = "Draft"
        };
}
