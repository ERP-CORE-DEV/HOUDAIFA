using Microsoft.Extensions.Logging;
using Training.SkillDevelopment.DTOs.Competency;
using Training.SkillDevelopment.DTOs.FinancementConformite.Cpf;
using Training.SkillDevelopment.DTOs.EntretienProfessionnel;
using Training.SkillDevelopment.DTOs.PilotageGouvernance.Gdpr;
using Training.SkillDevelopment.Repositories.FinancementConformite.Alternance;
using Training.SkillDevelopment.Repositories.BilanCompetences;
using Training.SkillDevelopment.Repositories.CertificationEcosysteme.Certification;
using Training.SkillDevelopment.Repositories.Competency;
using Training.SkillDevelopment.Repositories.FinancementConformite.Cpf;
using Training.SkillDevelopment.Repositories.EntretienProfessionnel;
using Training.SkillDevelopment.Repositories.FormationExecution.TrainingAction;

namespace Training.SkillDevelopment.Services.PilotageGouvernance.Gdpr;

public sealed class GdprService : IGdprService
{
    private const int DataRetentionYears = 6;

    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ICpfAccountRepository _cpfAccountRepository;
    private readonly ICompetencyAssessmentRepository _assessmentRepository;
    private readonly IProfessionalInterviewRepository _interviewRepository;
    private readonly IVaeProjectRepository _vaeProjectRepository;
    private readonly IBilanDeCompetencesRepository _bilanRepository;
    private readonly IAlternanceContractRepository _alternanceRepository;
    private readonly ILogger<GdprService> _logger;

    public GdprService(
        IEnrollmentRepository enrollmentRepository,
        ICpfAccountRepository cpfAccountRepository,
        ICompetencyAssessmentRepository assessmentRepository,
        IProfessionalInterviewRepository interviewRepository,
        IVaeProjectRepository vaeProjectRepository,
        IBilanDeCompetencesRepository bilanRepository,
        IAlternanceContractRepository alternanceRepository,
        ILogger<GdprService> logger)
    {
        ArgumentNullException.ThrowIfNull(enrollmentRepository);
        ArgumentNullException.ThrowIfNull(cpfAccountRepository);
        ArgumentNullException.ThrowIfNull(assessmentRepository);
        ArgumentNullException.ThrowIfNull(interviewRepository);
        ArgumentNullException.ThrowIfNull(vaeProjectRepository);
        ArgumentNullException.ThrowIfNull(bilanRepository);
        ArgumentNullException.ThrowIfNull(alternanceRepository);
        ArgumentNullException.ThrowIfNull(logger);

        _enrollmentRepository = enrollmentRepository;
        _cpfAccountRepository = cpfAccountRepository;
        _assessmentRepository = assessmentRepository;
        _interviewRepository = interviewRepository;
        _vaeProjectRepository = vaeProjectRepository;
        _bilanRepository = bilanRepository;
        _alternanceRepository = alternanceRepository;
        _logger = logger;
    }

    public async Task AnonymizeEmployeeDataAsync(string employeeId)
    {
        if (string.IsNullOrWhiteSpace(employeeId))
            throw new ArgumentException("L'identifiant de l'employe est obligatoire.", nameof(employeeId));

        _logger.LogInformation(
            "Demarrage de l'anonymisation RGPD pour l'employe {EmployeeId}.", employeeId);

        var enrollments = await _enrollmentRepository.GetByEmployeeIdAsync(employeeId);
        foreach (var enrollment in enrollments)
        {
            if (!enrollment.IsAnonymized)
            {
                enrollment.AnonymizePersonalData();
                await _enrollmentRepository.UpdateAsync(enrollment);
            }
        }

        var cpfAccount = await _cpfAccountRepository.GetByEmployeeIdAsync(employeeId);
        if (cpfAccount is not null && !cpfAccount.IsAnonymized)
        {
            cpfAccount.AnonymizePersonalData();
            await _cpfAccountRepository.UpdateAsync(cpfAccount);
        }

        var assessments = await _assessmentRepository.GetByEmployeeIdAsync(employeeId);
        foreach (var assessment in assessments)
        {
            if (!assessment.IsAnonymized)
            {
                assessment.AnonymizePersonalData();
                await _assessmentRepository.UpdateAsync(assessment);
            }
        }

        var interviews = await _interviewRepository.GetByEmployeeIdAsync(employeeId);
        foreach (var interview in interviews)
        {
            if (!interview.IsAnonymized)
            {
                interview.AnonymizePersonalData();
                await _interviewRepository.UpdateAsync(interview);
            }
        }

        var vaeProjects = await _vaeProjectRepository.GetByEmployeeIdAsync(employeeId);
        foreach (var project in vaeProjects)
        {
            if (!project.IsAnonymized)
            {
                project.AnonymizePersonalData();
                await _vaeProjectRepository.UpdateAsync(project);
            }
        }

        var bilans = await _bilanRepository.GetByEmployeeIdAsync(employeeId);
        foreach (var bilan in bilans)
        {
            if (!bilan.IsAnonymized)
            {
                bilan.AnonymizePersonalData();
                await _bilanRepository.UpdateAsync(bilan);
            }
        }

        var alternanceContracts = await _alternanceRepository.GetByEmployeeIdAsync(employeeId);
        foreach (var contract in alternanceContracts)
        {
            if (!contract.IsAnonymized)
            {
                contract.AnonymizePersonalData();
                await _alternanceRepository.UpdateAsync(contract);
            }
        }

        _logger.LogInformation(
            "Anonymisation RGPD terminee pour l'employe {EmployeeId}. " +
            "Inscriptions: {Enrollments}, Evaluations: {Assessments}, " +
            "Entretiens: {Interviews}, Projets VAE: {VaeProjects}, " +
            "Bilans: {Bilans}, Contrats alternance: {Alternance}.",
            employeeId,
            enrollments.Count,
            assessments.Count,
            interviews.Count,
            vaeProjects.Count,
            bilans.Count,
            alternanceContracts.Count);
    }

    public async Task<EmployeeTrainingDataExportDto> ExportEmployeeTrainingDataAsync(string employeeId)
    {
        if (string.IsNullOrWhiteSpace(employeeId))
            throw new ArgumentException("L'identifiant de l'employe est obligatoire.", nameof(employeeId));

        _logger.LogInformation(
            "Export RGPD (Art. 20) des donnees de formation pour l'employe {EmployeeId}.", employeeId);

        var enrollments = await _enrollmentRepository.GetByEmployeeIdAsync(employeeId);
        var enrollmentIds = enrollments.Select(e => e.Id).ToList();

        var cpfAccount = await _cpfAccountRepository.GetByEmployeeIdAsync(employeeId);
        var cpfTransactions = cpfAccount is not null
            ? new List<CpfTransactionDto>
              {
                  new()
                  {
                      Id = cpfAccount.Id,
                      AccountId = cpfAccount.Id,
                      Amount = cpfAccount.BalanceEuros,
                      Description = "Solde CPF",
                      Source = "MonCompteFormation",
                      TransactionDate = cpfAccount.UpdatedAt
                  }
              }
            : new List<CpfTransactionDto>();

        var assessments = await _assessmentRepository.GetByEmployeeIdAsync(employeeId);
        var assessmentDtos = assessments
            .Select(EmployeeCompetencyAssessmentDto.FromDomain)
            .ToList();

        var interviews = await _interviewRepository.GetByEmployeeIdAsync(employeeId);
        var interviewDtos = interviews
            .Select(ProfessionalInterviewDto.FromDomain)
            .ToList();

        var vaeProjects = await _vaeProjectRepository.GetByEmployeeIdAsync(employeeId);
        var vaeIds = vaeProjects.Select(v => v.Id).ToList();

        var bilans = await _bilanRepository.GetByEmployeeIdAsync(employeeId);
        var certificationIds = bilans.Select(b => b.Id).ToList();

        _logger.LogInformation(
            "Export RGPD genere pour l'employe {EmployeeId}: " +
            "{Enrollments} inscriptions, {Cpf} transactions CPF, " +
            "{Assessments} evaluations, {Interviews} entretiens.",
            employeeId,
            enrollmentIds.Count,
            cpfTransactions.Count,
            assessmentDtos.Count,
            interviewDtos.Count);

        return new EmployeeTrainingDataExportDto
        {
            EmployeeId = employeeId,
            ExportDate = DateTime.UtcNow,
            Enrollments = enrollmentIds.AsReadOnly(),
            CpfTransactions = cpfTransactions.AsReadOnly(),
            CompetencyAssessments = assessmentDtos.AsReadOnly(),
            Interviews = interviewDtos.AsReadOnly(),
            Certifications = certificationIds.AsReadOnly(),
            VaeProjects = vaeIds.AsReadOnly()
        };
    }

    public async Task<DataRetentionReportDto> GetDataRetentionStatusAsync()
    {
        _logger.LogInformation(
            "Generation du rapport de retention des donnees RGPD (politique CNIL: {Years} ans).",
            DataRetentionYears);

        var retentionCutoff = DateTime.UtcNow.AddYears(-DataRetentionYears);

        var allBilans = await _bilanRepository.GetAllAsync();
        var bilanExpired = allBilans.Count(b => b.CreatedAt < retentionCutoff);
        var bilanTotal = allBilans.Count;

        var allAlternance = await _alternanceRepository.GetAllAsync();
        var alternanceExpired = allAlternance.Count(a => a.CreatedAt < retentionCutoff);
        var alternanceTotal = allAlternance.Count;

        var totalAnalyzed = bilanTotal + alternanceTotal;
        var totalExpired = bilanExpired + alternanceExpired;
        var totalWithin = totalAnalyzed - totalExpired;

        var recordsByEntity = new Dictionary<string, int>
        {
            { "BilanDeCompetences", bilanTotal },
            { "AlternanceContract", alternanceTotal }
        };

        _logger.LogInformation(
            "Rapport de retention: {Total} enregistrements analyses, " +
            "{Expired} expires (anterieurs au {Cutoff:yyyy-MM-dd}).",
            totalAnalyzed, totalExpired, retentionCutoff);

        return new DataRetentionReportDto
        {
            ReportDate = DateTime.UtcNow,
            TotalRecordsAnalyzed = totalAnalyzed,
            RecordsWithinRetention = totalWithin,
            RecordsExpired = totalExpired,
            RecordsByEntity = recordsByEntity
        };
    }
}
