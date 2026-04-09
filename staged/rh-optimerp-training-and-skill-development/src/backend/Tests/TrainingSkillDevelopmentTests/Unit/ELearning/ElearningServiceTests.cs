using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.FormationExecution.ELearning;
using Training.SkillDevelopment.Repositories.FormationExecution.ELearning;
using Training.SkillDevelopment.Services.FormationExecution.ELearning;
using Xunit;

namespace Training.SkillDevelopment.Tests.Unit.ELearning;

public sealed class ElearningServiceTests
{
    private readonly Mock<IElearningCourseRepository> _repositoryMock;
    private readonly Mock<ILogger<ElearningService>> _loggerMock;
    private readonly ElearningService _sut;

    public ElearningServiceTests()
    {
        _repositoryMock = new Mock<IElearningCourseRepository>();
        _loggerMock = new Mock<ILogger<ElearningService>>();
        _sut = new ElearningService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidCourse_ReturnsCreatedCourse()
    {
        // Arrange
        var course = BuildValidCourse();
        var created = BuildValidCourse();
        created.Id = Guid.NewGuid().ToString();
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ElearningCourse>()))
            .ReturnsAsync(created);

        // Act
        var result = await _sut.CreateAsync(course);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateAsync_EmptyTitle_ThrowsArgumentException()
    {
        // Arrange
        var course = BuildValidCourse();
        course.Title = string.Empty;

        // Act
        var act = async () => await _sut.CreateAsync(course);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*titre*");
    }

    [Fact]
    public async Task CreateAsync_EmptyFormat_ThrowsArgumentException()
    {
        // Arrange
        var course = BuildValidCourse();
        course.Format = string.Empty;

        // Act
        var act = async () => await _sut.CreateAsync(course);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*format*");
    }

    [Fact]
    public async Task CreateAsync_ZeroDurationMinutes_ThrowsArgumentException()
    {
        // Arrange
        var course = BuildValidCourse();
        course.DurationMinutes = 0;

        // Act
        var act = async () => await _sut.CreateAsync(course);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*duree*");
    }

    [Fact]
    public async Task CreateAsync_NegativeDurationMinutes_ThrowsArgumentException()
    {
        // Arrange
        var course = BuildValidCourse();
        course.DurationMinutes = -10;

        // Act
        var act = async () => await _sut.CreateAsync(course);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateAsync_SetsIsActiveTrue_OnNewCourse()
    {
        // Arrange
        var course = BuildValidCourse();
        ElearningCourse? captured = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ElearningCourse>()))
            .Callback<ElearningCourse>(c => captured = c)
            .ReturnsAsync((ElearningCourse c) => c);

        // Act
        await _sut.CreateAsync(course);

        // Assert
        captured!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCourse()
    {
        // Arrange
        var expected = BuildValidCourse();
        expected.Id = "course-001";
        _repositoryMock
            .Setup(r => r.GetByIdAsync("course-001"))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.GetByIdAsync("course-001");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("course-001");
    }

    [Fact]
    public async Task GetByIdAsync_EmptyId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.GetByIdAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*identifiant*");
    }

    [Fact]
    public async Task TrackProgressAsync_100PercentProgress_SetsCompletedStatus()
    {
        // Arrange
        var progress = new LearnerProgress
        {
            EmployeeId = "emp-001",
            CourseId = "course-001",
            ProgressPercentage = 100,
            Status = "InProgress"
        };

        // Act
        var result = await _sut.TrackProgressAsync(progress);

        // Assert
        result.Status.Should().Be("Completed");
        result.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task TrackProgressAsync_PartialProgressFromNotStarted_SetsInProgressStatus()
    {
        // Arrange
        var progress = new LearnerProgress
        {
            EmployeeId = "emp-001",
            CourseId = "course-001",
            ProgressPercentage = 50,
            Status = "NotStarted"
        };

        // Act
        var result = await _sut.TrackProgressAsync(progress);

        // Assert
        result.Status.Should().Be("InProgress");
        result.StartedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task TrackProgressAsync_NegativeProgress_ThrowsArgumentException()
    {
        // Arrange
        var progress = new LearnerProgress
        {
            EmployeeId = "emp-001",
            CourseId = "course-001",
            ProgressPercentage = -5
        };

        // Act
        var act = async () => await _sut.TrackProgressAsync(progress);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*pourcentage*");
    }

    [Fact]
    public async Task TrackProgressAsync_ProgressOver100_ThrowsArgumentException()
    {
        // Arrange
        var progress = new LearnerProgress
        {
            EmployeeId = "emp-001",
            CourseId = "course-001",
            ProgressPercentage = 101
        };

        // Act
        var act = async () => await _sut.TrackProgressAsync(progress);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task TrackProgressAsync_MissingEmployeeId_ThrowsArgumentException()
    {
        // Arrange
        var progress = new LearnerProgress
        {
            EmployeeId = string.Empty,
            CourseId = "course-001",
            ProgressPercentage = 50
        };

        // Act
        var act = async () => await _sut.TrackProgressAsync(progress);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*employe*");
    }

    [Fact]
    public async Task GetCompletionRateAsync_ExistingCourse_ReturnsRate()
    {
        // Arrange
        var course = BuildValidCourse();
        course.Id = "course-001";
        _repositoryMock
            .Setup(r => r.GetByIdAsync("course-001"))
            .ReturnsAsync(course);

        // Act
        var result = await _sut.GetCompletionRateAsync("course-001");

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0m);
    }

    [Fact]
    public async Task GetCompletionRateAsync_NonExistentCourse_ThrowsKeyNotFoundException()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync("not-found"))
            .ReturnsAsync((ElearningCourse?)null);

        // Act
        var act = async () => await _sut.GetCompletionRateAsync("not-found");

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetByTagAsync_EmptyTag_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.GetByTagAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*tag*");
    }

    [Fact]
    public async Task UpdateAsync_NonExistentCourse_ThrowsKeyNotFoundException()
    {
        // Arrange
        var course = BuildValidCourse();
        course.Id = "not-found";
        _repositoryMock
            .Setup(r => r.GetByIdAsync("not-found"))
            .ReturnsAsync((ElearningCourse?)null);

        // Act
        var act = async () => await _sut.UpdateAsync(course);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    private static ElearningCourse BuildValidCourse() =>
        new()
        {
            Title = "Introduction au droit du travail francais",
            Format = "SCORM",
            DurationMinutes = 120,
            IsActive = true
        };
}
