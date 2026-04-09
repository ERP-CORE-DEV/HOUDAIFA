using System.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Training.SkillDevelopment.Models.CertificationEcosysteme.Certification;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.Competency;
using Training.SkillDevelopment.Repositories.CertificationEcosysteme.Certification;
using Training.SkillDevelopment.Repositories.Competency;
using Training.SkillDevelopment.Repositories.FormationExecution.TrainingPlan;
using Xunit;

namespace Training.SkillDevelopment.Tests.Performance;

/// <summary>
/// Tests verifying that InMemory repository operations meet the performance
/// SLAs defined in the architecture: single entity ops &lt; 100ms,
/// batch ops (100 items) &lt; 2s, search ops &lt; 200ms.
/// </summary>
public sealed class RepositoryPerformanceTests
{
    private const int SingleEntityLimitMs = 100;
    private const int BatchLimitMs = 2000;
    private const int SearchLimitMs = 200;
    private const int BatchSize = 100;

    // -------------------------------------------------------------------------
    // TrainingPlan repository
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateTrainingPlan_SingleEntity_ShouldCompleteWithin100ms()
    {
        // Arrange
        var repository = new InMemoryTrainingPlanRepository(NullLogger<InMemoryTrainingPlanRepository>.Instance);
        var plan = BuildTrainingPlan();
        var stopwatch = Stopwatch.StartNew();

        // Act
        await repository.AddAsync(plan);

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SingleEntityLimitMs));
    }

    [Fact]
    public async Task ReadTrainingPlanById_SingleEntity_ShouldCompleteWithin100ms()
    {
        // Arrange
        var repository = new InMemoryTrainingPlanRepository(NullLogger<InMemoryTrainingPlanRepository>.Instance);
        var plan = await repository.AddAsync(BuildTrainingPlan());

        var stopwatch = Stopwatch.StartNew();

        // Act
        await repository.GetByIdAsync(plan.Id);

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SingleEntityLimitMs));
    }

    [Fact]
    public async Task UpdateTrainingPlan_SingleEntity_ShouldCompleteWithin100ms()
    {
        // Arrange
        var repository = new InMemoryTrainingPlanRepository(NullLogger<InMemoryTrainingPlanRepository>.Instance);
        var plan = await repository.AddAsync(BuildTrainingPlan());
        plan.Title = "Plan Mis a Jour Performance";

        var stopwatch = Stopwatch.StartNew();

        // Act
        await repository.UpdateAsync(plan);

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SingleEntityLimitMs));
    }

    [Fact]
    public async Task DeleteTrainingPlan_SingleEntity_ShouldCompleteWithin100ms()
    {
        // Arrange
        var repository = new InMemoryTrainingPlanRepository(NullLogger<InMemoryTrainingPlanRepository>.Instance);
        var plan = await repository.AddAsync(BuildTrainingPlan());

        var stopwatch = Stopwatch.StartNew();

        // Act
        await repository.DeleteAsync(plan.Id);

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SingleEntityLimitMs));
    }

    [Fact]
    public async Task BatchCreateTrainingPlans_100Entities_ShouldCompleteWithin2s()
    {
        // Arrange
        var repository = new InMemoryTrainingPlanRepository(NullLogger<InMemoryTrainingPlanRepository>.Instance);
        var plans = Enumerable.Range(0, BatchSize).Select(_ => BuildTrainingPlan()).ToList();

        var stopwatch = Stopwatch.StartNew();

        // Act
        foreach (var plan in plans)
        {
            await repository.AddAsync(plan);
        }

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(BatchLimitMs));
    }

    [Fact]
    public async Task GetPagedTrainingPlans_QueryOperation_ShouldCompleteWithin200ms()
    {
        // Arrange — seed 50 plans so the query is non-trivial
        var repository = new InMemoryTrainingPlanRepository(NullLogger<InMemoryTrainingPlanRepository>.Instance);
        for (var i = 0; i < 50; i++)
        {
            await repository.AddAsync(BuildTrainingPlan());
        }

        var stopwatch = Stopwatch.StartNew();

        // Act
        await repository.GetPagedAsync(page: 1, pageSize: 20);

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SearchLimitMs));
    }

    // -------------------------------------------------------------------------
    // Competency repository
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateCompetency_SingleEntity_ShouldCompleteWithin100ms()
    {
        // Arrange
        var repository = new InMemoryCompetencyRepository();
        var competency = BuildCompetency();
        var stopwatch = Stopwatch.StartNew();

        // Act
        await repository.CreateAsync(competency);

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SingleEntityLimitMs));
    }

    [Fact]
    public async Task ReadCompetencyById_SingleEntity_ShouldCompleteWithin100ms()
    {
        // Arrange
        var repository = new InMemoryCompetencyRepository();
        var competency = await repository.CreateAsync(BuildCompetency());

        var stopwatch = Stopwatch.StartNew();

        // Act
        await repository.GetByIdAsync(competency.Id);

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SingleEntityLimitMs));
    }

    [Fact]
    public async Task UpdateCompetency_SingleEntity_ShouldCompleteWithin100ms()
    {
        // Arrange
        var repository = new InMemoryCompetencyRepository();
        var competency = await repository.CreateAsync(BuildCompetency());
        competency.Name = "Competence Mise a Jour Performance";

        var stopwatch = Stopwatch.StartNew();

        // Act
        await repository.UpdateAsync(competency);

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SingleEntityLimitMs));
    }

    [Fact]
    public async Task DeleteCompetency_SingleEntity_ShouldCompleteWithin100ms()
    {
        // Arrange
        var repository = new InMemoryCompetencyRepository();
        var competency = await repository.CreateAsync(BuildCompetency());

        var stopwatch = Stopwatch.StartNew();

        // Act
        await repository.DeleteAsync(competency.Id);

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SingleEntityLimitMs));
    }

    [Fact]
    public async Task BatchCreateCompetencies_100Entities_ShouldCompleteWithin2s()
    {
        // Arrange
        var repository = new InMemoryCompetencyRepository();
        var competencies = Enumerable.Range(0, BatchSize).Select(_ => BuildCompetency()).ToList();

        var stopwatch = Stopwatch.StartNew();

        // Act
        foreach (var competency in competencies)
        {
            await repository.CreateAsync(competency);
        }

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(BatchLimitMs));
    }

    [Fact]
    public async Task SearchCompetencies_QueryOperation_ShouldCompleteWithin200ms()
    {
        // Arrange — seed 50 competencies so the search is non-trivial
        var repository = new InMemoryCompetencyRepository();
        for (var i = 0; i < 50; i++)
        {
            await repository.CreateAsync(BuildCompetency(domain: "Securite"));
        }

        var stopwatch = Stopwatch.StartNew();

        // Act
        await repository.SearchAsync(searchTerm: "securite", page: 1, pageSize: 20);

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SearchLimitMs));
    }

    [Fact]
    public async Task GetCompetenciesByDomain_QueryOperation_ShouldCompleteWithin200ms()
    {
        // Arrange
        var repository = new InMemoryCompetencyRepository();
        for (var i = 0; i < 50; i++)
        {
            await repository.CreateAsync(BuildCompetency(domain: "Performance-Domain"));
        }

        var stopwatch = Stopwatch.StartNew();

        // Act
        await repository.GetByDomainAsync(domain: "Performance-Domain", page: 1, pageSize: 20);

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SearchLimitMs));
    }

    // -------------------------------------------------------------------------
    // Certification repository
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateCertification_SingleEntity_ShouldCompleteWithin100ms()
    {
        // Arrange
        var repository = new InMemoryCertificationRepository(NullLogger<InMemoryCertificationRepository>.Instance);
        var certification = BuildCertification();
        var stopwatch = Stopwatch.StartNew();

        // Act
        await repository.AddAsync(certification);

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SingleEntityLimitMs));
    }

    [Fact]
    public async Task ReadCertificationById_SingleEntity_ShouldCompleteWithin100ms()
    {
        // Arrange
        var repository = new InMemoryCertificationRepository(NullLogger<InMemoryCertificationRepository>.Instance);
        var certification = await repository.AddAsync(BuildCertification());

        var stopwatch = Stopwatch.StartNew();

        // Act
        await repository.GetByIdAsync(certification.Id);

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SingleEntityLimitMs));
    }

    [Fact]
    public async Task UpdateCertification_SingleEntity_ShouldCompleteWithin100ms()
    {
        // Arrange
        var repository = new InMemoryCertificationRepository(NullLogger<InMemoryCertificationRepository>.Instance);
        var certification = await repository.AddAsync(BuildCertification());
        certification.Title = "Certification Mise a Jour Performance";

        var stopwatch = Stopwatch.StartNew();

        // Act
        await repository.UpdateAsync(certification);

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SingleEntityLimitMs));
    }

    [Fact]
    public async Task DeleteCertification_SingleEntity_ShouldCompleteWithin100ms()
    {
        // Arrange
        var repository = new InMemoryCertificationRepository(NullLogger<InMemoryCertificationRepository>.Instance);
        var certification = await repository.AddAsync(BuildCertification());

        var stopwatch = Stopwatch.StartNew();

        // Act
        await repository.DeleteAsync(certification.Id);

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SingleEntityLimitMs));
    }

    [Fact]
    public async Task BatchCreateCertifications_100Entities_ShouldCompleteWithin2s()
    {
        // Arrange
        var repository = new InMemoryCertificationRepository(NullLogger<InMemoryCertificationRepository>.Instance);
        var certifications = Enumerable.Range(0, BatchSize).Select(_ => BuildCertification()).ToList();

        var stopwatch = Stopwatch.StartNew();

        // Act
        foreach (var certification in certifications)
        {
            await repository.AddAsync(certification);
        }

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(BatchLimitMs));
    }

    [Fact]
    public async Task GetAllCertifications_QueryOperation_ShouldCompleteWithin200ms()
    {
        // Arrange — seed 50 certifications so the query is non-trivial
        var repository = new InMemoryCertificationRepository(NullLogger<InMemoryCertificationRepository>.Instance);
        for (var i = 0; i < 50; i++)
        {
            await repository.AddAsync(BuildCertification());
        }

        var stopwatch = Stopwatch.StartNew();

        // Act
        await repository.GetAllAsync();

        // Assert
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(SearchLimitMs));
    }

    // -------------------------------------------------------------------------
    // Builders
    // -------------------------------------------------------------------------

    private static Models.FormationExecution.TrainingPlan.TrainingPlan BuildTrainingPlan() =>
        new()
        {
            Id = Guid.NewGuid().ToString(),
            CompanyId = "company-perf-001",
            Title = $"Plan de Formation Performance {Guid.NewGuid():N}"[..40],
            Year = 2026,
            BudgetAllocated = 50_000m,
            MasseSalariale = 0m,
            LegalObligationRate = 0.01m,
            Status = TrainingPlanStatus.Draft,
            IsActive = true
        };

    private static Competency BuildCompetency(string domain = "Performance") =>
        new()
        {
            Id = Guid.NewGuid().ToString(),
            Code = $"COMP-PERF-{Guid.NewGuid():N}"[..16],
            Name = $"Competence Securite Performance {Guid.NewGuid():N}"[..35],
            Description = "Competence generee pour les tests de performance.",
            Domain = domain,
            Family = "Tests",
            Type = CompetencyType.Technical,
            IsCritical = false,
            IsActive = true,
            Version = 1
        };

    private static CertificationRncp BuildCertification()
    {
        var uniqueNumeric = Math.Abs(Guid.NewGuid().GetHashCode()) % 900_000 + 10_000;
        return new CertificationRncp
        {
            Id = Guid.NewGuid().ToString(),
            RncpCode = $"RNCP{uniqueNumeric}",
            Title = "BTS Services Informatiques aux Organisations - Performance Test",
            Description = "Certification generee pour les tests de performance.",
            CertifyingBody = "Ministere de l'Education Nationale",
            NsfCode = 326,
            Level = "5",
            RegistrationDate = new DateTime(2021, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            ExpirationDate = new DateTime(2026, 8, 31, 0, 0, 0, DateTimeKind.Utc),
            IsActive = true
        };
    }
}
