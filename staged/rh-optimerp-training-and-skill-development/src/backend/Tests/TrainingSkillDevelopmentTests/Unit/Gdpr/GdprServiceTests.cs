using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Training.SkillDevelopment.Models.FinancementConformite.Alternance;
using Training.SkillDevelopment.Models.BilanCompetences;
using Training.SkillDevelopment.Models.CertificationEcosysteme.Certification;
using Training.SkillDevelopment.Models.Common;
using Training.SkillDevelopment.Models.Competency;
using Training.SkillDevelopment.Models.FinancementConformite.Cpf;
using Training.SkillDevelopment.Models.EntretienProfessionnel;
using Training.SkillDevelopment.Models.FormationExecution.TrainingAction;
using Training.SkillDevelopment.Repositories.FinancementConformite.Alternance;
using Training.SkillDevelopment.Repositories.BilanCompetences;
using Training.SkillDevelopment.Repositories.CertificationEcosysteme.Certification;
using Training.SkillDevelopment.Repositories.Competency;
using Training.SkillDevelopment.Repositories.FinancementConformite.Cpf;
using Training.SkillDevelopment.Repositories.EntretienProfessionnel;
using Training.SkillDevelopment.Repositories.FormationExecution.TrainingAction;
using Training.SkillDevelopment.Services.PilotageGouvernance.Gdpr;
using Xunit;

namespace Training.SkillDevelopment.Tests.Unit.Gdpr;

public sealed class GdprServiceTests
{
    private readonly Mock<IEnrollmentRepository> _enrollmentRepositoryMock;
    private readonly Mock<ICpfAccountRepository> _cpfAccountRepositoryMock;
    private readonly Mock<ICompetencyAssessmentRepository> _assessmentRepositoryMock;
    private readonly Mock<IProfessionalInterviewRepository> _interviewRepositoryMock;
    private readonly Mock<IVaeProjectRepository> _vaeProjectRepositoryMock;
    private readonly Mock<IBilanDeCompetencesRepository> _bilanRepositoryMock;
    private readonly Mock<IAlternanceContractRepository> _alternanceRepositoryMock;
    private readonly Mock<ILogger<GdprService>> _loggerMock;
    private readonly GdprService _sut;

    public GdprServiceTests()
    {
        _enrollmentRepositoryMock = new Mock<IEnrollmentRepository>();
        _cpfAccountRepositoryMock = new Mock<ICpfAccountRepository>();
        _assessmentRepositoryMock = new Mock<ICompetencyAssessmentRepository>();
        _interviewRepositoryMock = new Mock<IProfessionalInterviewRepository>();
        _vaeProjectRepositoryMock = new Mock<IVaeProjectRepository>();
        _bilanRepositoryMock = new Mock<IBilanDeCompetencesRepository>();
        _alternanceRepositoryMock = new Mock<IAlternanceContractRepository>();
        _loggerMock = new Mock<ILogger<GdprService>>();

        _sut = new GdprService(
            _enrollmentRepositoryMock.Object,
            _cpfAccountRepositoryMock.Object,
            _assessmentRepositoryMock.Object,
            _interviewRepositoryMock.Object,
            _vaeProjectRepositoryMock.Object,
            _bilanRepositoryMock.Object,
            _alternanceRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task AnonymizeEmployeeDataAsync_EmptyEmployeeId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.AnonymizeEmployeeDataAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*employe*");
    }

    [Fact]
    public async Task AnonymizeEmployeeDataAsync_WhitespaceEmployeeId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.AnonymizeEmployeeDataAsync("   ");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AnonymizeEmployeeDataAsync_ValidEmployee_AnonymizesEnrollments()
    {
        // Arrange
        SetupEmptyRepositories("emp-001");
        var enrollment = BuildEnrollment("emp-001");
        _enrollmentRepositoryMock
            .Setup(r => r.GetByEmployeeIdAsync("emp-001"))
            .ReturnsAsync(new List<Enrollment> { enrollment });

        // Act
        await _sut.AnonymizeEmployeeDataAsync("emp-001");

        // Assert
        _enrollmentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Enrollment>()), Times.Once);
    }

    [Fact]
    public async Task AnonymizeEmployeeDataAsync_AlreadyAnonymizedEnrollment_DoesNotUpdateAgain()
    {
        // Arrange
        SetupEmptyRepositories("emp-001");
        var enrollment = BuildEnrollment("emp-001");
        enrollment.IsAnonymized = true;
        enrollment.AnonymizationDate = DateTime.UtcNow.AddDays(-10);
        _enrollmentRepositoryMock
            .Setup(r => r.GetByEmployeeIdAsync("emp-001"))
            .ReturnsAsync(new List<Enrollment> { enrollment });

        // Act
        await _sut.AnonymizeEmployeeDataAsync("emp-001");

        // Assert
        _enrollmentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Enrollment>()), Times.Never);
    }

    [Fact]
    public async Task AnonymizeEmployeeDataAsync_ValidEmployee_AnonymizesCpfAccount()
    {
        // Arrange
        SetupEmptyRepositories("emp-001");
        var cpfAccount = BuildCpfAccount("emp-001");
        _cpfAccountRepositoryMock
            .Setup(r => r.GetByEmployeeIdAsync("emp-001", default))
            .ReturnsAsync(cpfAccount);

        // Act
        await _sut.AnonymizeEmployeeDataAsync("emp-001");

        // Assert
        _cpfAccountRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<CpfAccount>(), default), Times.Once);
    }

    [Fact]
    public async Task AnonymizeEmployeeDataAsync_NoCpfAccount_DoesNotThrow()
    {
        // Arrange
        SetupEmptyRepositories("emp-001");
        _cpfAccountRepositoryMock
            .Setup(r => r.GetByEmployeeIdAsync("emp-001", default))
            .ReturnsAsync((CpfAccount?)null);

        // Act
        var act = async () => await _sut.AnonymizeEmployeeDataAsync("emp-001");

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task AnonymizeEmployeeDataAsync_ValidEmployee_AnonymizesInterviews()
    {
        // Arrange
        SetupEmptyRepositories("emp-001");
        var interview = BuildInterview("emp-001");
        _interviewRepositoryMock
            .Setup(r => r.GetByEmployeeIdAsync("emp-001", default))
            .ReturnsAsync(new List<ProfessionalInterview> { interview });

        // Act
        await _sut.AnonymizeEmployeeDataAsync("emp-001");

        // Assert
        _interviewRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ProfessionalInterview>(), default), Times.Once);
    }

    [Fact]
    public async Task AnonymizeEmployeeDataAsync_ValidEmployee_AnonymizesVaeProjects()
    {
        // Arrange
        SetupEmptyRepositories("emp-001");
        var vaeProject = BuildVaeProject("emp-001");
        _vaeProjectRepositoryMock
            .Setup(r => r.GetByEmployeeIdAsync("emp-001"))
            .ReturnsAsync(new List<VaeProject> { vaeProject });

        // Act
        await _sut.AnonymizeEmployeeDataAsync("emp-001");

        // Assert
        _vaeProjectRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<VaeProject>()), Times.Once);
    }

    [Fact]
    public async Task AnonymizeEmployeeDataAsync_ValidEmployee_AnonymizesBilans()
    {
        // Arrange
        SetupEmptyRepositories("emp-001");
        var bilan = BuildBilan("emp-001");
        _bilanRepositoryMock
            .Setup(r => r.GetByEmployeeIdAsync("emp-001"))
            .ReturnsAsync(new List<BilanDeCompetences> { bilan });

        // Act
        await _sut.AnonymizeEmployeeDataAsync("emp-001");

        // Assert
        _bilanRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<BilanDeCompetences>()), Times.Once);
    }

    [Fact]
    public async Task AnonymizeEmployeeDataAsync_ValidEmployee_AnonymizesAlternanceContracts()
    {
        // Arrange
        SetupEmptyRepositories("emp-001");
        var contract = BuildAlternanceContract("emp-001");
        _alternanceRepositoryMock
            .Setup(r => r.GetByEmployeeIdAsync("emp-001"))
            .ReturnsAsync(new List<AlternanceContract> { contract });

        // Act
        await _sut.AnonymizeEmployeeDataAsync("emp-001");

        // Assert
        _alternanceRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<AlternanceContract>()), Times.Once);
    }

    [Fact]
    public async Task ExportEmployeeTrainingDataAsync_EmptyEmployeeId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = async () => await _sut.ExportEmployeeTrainingDataAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*employe*");
    }

    [Fact]
    public async Task ExportEmployeeTrainingDataAsync_ValidEmployee_ReturnsExportWithCorrectEmployeeId()
    {
        // Arrange
        SetupEmptyRepositories("emp-001");

        // Act
        var result = await _sut.ExportEmployeeTrainingDataAsync("emp-001");

        // Assert
        result.EmployeeId.Should().Be("emp-001");
        result.ExportDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ExportEmployeeTrainingDataAsync_WithEnrollments_ExportsEnrollmentIds()
    {
        // Arrange
        SetupEmptyRepositories("emp-001");
        var enrollments = new List<Enrollment>
        {
            BuildEnrollment("emp-001"),
            BuildEnrollment("emp-001")
        };
        _enrollmentRepositoryMock
            .Setup(r => r.GetByEmployeeIdAsync("emp-001"))
            .ReturnsAsync(enrollments);

        // Act
        var result = await _sut.ExportEmployeeTrainingDataAsync("emp-001");

        // Assert
        result.Enrollments.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExportEmployeeTrainingDataAsync_WithVaeProjects_ExportsVaeIds()
    {
        // Arrange
        SetupEmptyRepositories("emp-001");
        _vaeProjectRepositoryMock
            .Setup(r => r.GetByEmployeeIdAsync("emp-001"))
            .ReturnsAsync(new List<VaeProject> { BuildVaeProject("emp-001") });

        // Act
        var result = await _sut.ExportEmployeeTrainingDataAsync("emp-001");

        // Assert
        result.VaeProjects.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetDataRetentionStatusAsync_ReturnsReport()
    {
        // Arrange
        _bilanRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<BilanDeCompetences>());
        _alternanceRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<AlternanceContract>());

        // Act
        var result = await _sut.GetDataRetentionStatusAsync();

        // Assert
        result.Should().NotBeNull();
        result.ReportDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetDataRetentionStatusAsync_WithOldRecords_MarksThemAsExpired()
    {
        // Arrange
        var oldBilan = BuildBilan("emp-001");
        oldBilan.CreatedAt = DateTime.UtcNow.AddYears(-7);

        _bilanRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<BilanDeCompetences> { oldBilan });
        _alternanceRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<AlternanceContract>());

        // Act
        var result = await _sut.GetDataRetentionStatusAsync();

        // Assert
        result.RecordsExpired.Should().Be(1);
        result.RecordsWithinRetention.Should().Be(0);
    }

    [Fact]
    public async Task GetDataRetentionStatusAsync_WithRecentRecords_MarksThemWithinRetention()
    {
        // Arrange
        var recentBilan = BuildBilan("emp-001");
        recentBilan.CreatedAt = DateTime.UtcNow.AddYears(-2);

        _bilanRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<BilanDeCompetences> { recentBilan });
        _alternanceRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<AlternanceContract>());

        // Act
        var result = await _sut.GetDataRetentionStatusAsync();

        // Assert
        result.RecordsWithinRetention.Should().Be(1);
        result.RecordsExpired.Should().Be(0);
    }

    [Fact]
    public async Task GetDataRetentionStatusAsync_ReportContainsEntityBreakdown()
    {
        // Arrange
        _bilanRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<BilanDeCompetences>());
        _alternanceRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<AlternanceContract>());

        // Act
        var result = await _sut.GetDataRetentionStatusAsync();

        // Assert
        result.RecordsByEntity.Should().ContainKey("BilanDeCompetences");
        result.RecordsByEntity.Should().ContainKey("AlternanceContract");
    }

    [Fact]
    public async Task GetDataRetentionStatusAsync_TotalAnalyzedEqualsWithinPlusExpired()
    {
        // Arrange
        var oldBilan = BuildBilan("emp-001");
        oldBilan.CreatedAt = DateTime.UtcNow.AddYears(-7);
        var recentBilan = BuildBilan("emp-002");
        recentBilan.CreatedAt = DateTime.UtcNow.AddYears(-1);

        _bilanRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<BilanDeCompetences> { oldBilan, recentBilan });
        _alternanceRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<AlternanceContract>());

        // Act
        var result = await _sut.GetDataRetentionStatusAsync();

        // Assert
        result.TotalRecordsAnalyzed.Should().Be(result.RecordsWithinRetention + result.RecordsExpired);
        result.TotalRecordsAnalyzed.Should().Be(2);
    }

    private void SetupEmptyRepositories(string employeeId)
    {
        _enrollmentRepositoryMock
            .Setup(r => r.GetByEmployeeIdAsync(employeeId))
            .ReturnsAsync(new List<Enrollment>());
        _cpfAccountRepositoryMock
            .Setup(r => r.GetByEmployeeIdAsync(employeeId, default))
            .ReturnsAsync((CpfAccount?)null);
        _assessmentRepositoryMock
            .Setup(r => r.GetByEmployeeIdAsync(employeeId, default))
            .ReturnsAsync(new List<EmployeeCompetencyAssessment>());
        _interviewRepositoryMock
            .Setup(r => r.GetByEmployeeIdAsync(employeeId, default))
            .ReturnsAsync(new List<ProfessionalInterview>());
        _vaeProjectRepositoryMock
            .Setup(r => r.GetByEmployeeIdAsync(employeeId))
            .ReturnsAsync(new List<VaeProject>());
        _bilanRepositoryMock
            .Setup(r => r.GetByEmployeeIdAsync(employeeId))
            .ReturnsAsync(new List<BilanDeCompetences>());
        _alternanceRepositoryMock
            .Setup(r => r.GetByEmployeeIdAsync(employeeId))
            .ReturnsAsync(new List<AlternanceContract>());
    }

    private static Enrollment BuildEnrollment(string employeeId) =>
        new()
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = "session-001",
            EmployeeId = employeeId,
            Status = EnrollmentStatus.Attended,
            EnrolledAt = DateTime.UtcNow
        };

    private static CpfAccount BuildCpfAccount(string employeeId) =>
        new()
        {
            Id = Guid.NewGuid().ToString(),
            EmployeeId = employeeId,
            BalanceEuros = 1500m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    private static ProfessionalInterview BuildInterview(string employeeId) =>
        new()
        {
            Id = Guid.NewGuid().ToString(),
            EmployeeId = employeeId,
            ManagerId = "manager-001",
            Type = InterviewType.Biennial,
            ScheduledDate = DateTime.UtcNow,
            Status = InterviewStatus.Conducted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    private static VaeProject BuildVaeProject(string employeeId) =>
        new()
        {
            Id = Guid.NewGuid().ToString(),
            EmployeeId = employeeId,
            Status = VaeStatus.Accompagnement,
            StartDate = DateTime.UtcNow.AddMonths(-6),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    private static BilanDeCompetences BuildBilan(string employeeId) =>
        new()
        {
            Id = Guid.NewGuid().ToString(),
            EmployeeId = employeeId,
            Status = BilanStatus.Completed,
            TotalHours = 24,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    private static AlternanceContract BuildAlternanceContract(string employeeId) =>
        new()
        {
            Id = Guid.NewGuid().ToString(),
            EmployeeId = employeeId,
            Type = AlternanceContractType.Apprentissage,
            StartDate = DateTime.UtcNow.AddYears(-1),
            EndDate = DateTime.UtcNow.AddYears(1),
            Status = "Active",
            RemunerationPercentage = 60m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
}
