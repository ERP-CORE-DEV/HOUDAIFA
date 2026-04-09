using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Training.SkillDevelopment.Repositories.FinancementConformite.Alternance;
using Training.SkillDevelopment.Repositories.BilanCompetences;
using Training.SkillDevelopment.Repositories.CertificationEcosysteme.Certification;
using Training.SkillDevelopment.Repositories.Competency;
using Training.SkillDevelopment.Repositories.FinancementConformite.Cpf;
using Training.SkillDevelopment.Repositories.FormationExecution.ELearning;
using Training.SkillDevelopment.Repositories.EntretienProfessionnel;
using Training.SkillDevelopment.Repositories.FormationExecution.Evaluation;
using Training.SkillDevelopment.Repositories.FinancementConformite.Opco;
using Training.SkillDevelopment.Repositories.CertificationEcosysteme.Provider;
using Training.SkillDevelopment.Repositories.FormationExecution.TrainingAction;
using Training.SkillDevelopment.Repositories.FormationExecution.TrainingPlan;
using Training.SkillDevelopment.Repositories.Tutorat;
using Training.SkillDevelopment.Repositories.FinancementConformite.Compliance;

namespace Training.SkillDevelopment.Tests.Integration;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Use Development so Program.cs falls back to InMemory repositories when CosmosDB is absent.
        // The factory then replaces those registrations with fresh InMemory singletons below.
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Remove CosmosDB client registration
            services.RemoveAll<CosmosClient>();

            // Remove all Cosmos repository registrations and replace with InMemory
            ReplaceWithInMemoryRepositories(services);

            // Replace authentication with test scheme that auto-authenticates
            services.AddAuthentication("TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
            });
        });
    }

    private static void ReplaceWithInMemoryRepositories(IServiceCollection services)
    {
        services.RemoveAll<ITrainingPlanRepository>();
        services.RemoveAll<ITrainingActionRepository>();
        services.RemoveAll<ITrainingSessionRepository>();
        services.RemoveAll<IEnrollmentRepository>();
        services.RemoveAll<ICpfAccountRepository>();
        services.RemoveAll<ICompetencyRepository>();
        services.RemoveAll<ICompetencyAssessmentRepository>();
        services.RemoveAll<IProfessionalInterviewRepository>();
        services.RemoveAll<IOpcoRepository>();
        services.RemoveAll<IFundingApplicationRepository>();
        services.RemoveAll<ICertificationRepository>();
        services.RemoveAll<IVaeProjectRepository>();
        services.RemoveAll<IBilanDeCompetencesRepository>();
        services.RemoveAll<IElearningCourseRepository>();
        services.RemoveAll<ITrainingEvaluationRepository>();
        services.RemoveAll<ITrainingProviderRepository>();
        services.RemoveAll<ITutoringProgramRepository>();
        services.RemoveAll<IAlternanceContractRepository>();
        services.RemoveAll<ITrainingObligationRepository>();

        services.AddSingleton<ITrainingPlanRepository, InMemoryTrainingPlanRepository>();
        services.AddSingleton<ITrainingActionRepository, InMemoryTrainingActionRepository>();
        services.AddSingleton<ITrainingSessionRepository, InMemoryTrainingSessionRepository>();
        services.AddSingleton<IEnrollmentRepository, InMemoryEnrollmentRepository>();
        services.AddSingleton<ICpfAccountRepository, InMemoryCpfAccountRepository>();
        services.AddSingleton<ICompetencyRepository, InMemoryCompetencyRepository>();
        services.AddSingleton<ICompetencyAssessmentRepository, InMemoryCompetencyAssessmentRepository>();
        services.AddSingleton<IProfessionalInterviewRepository, InMemoryProfessionalInterviewRepository>();
        services.AddSingleton<IOpcoRepository, InMemoryOpcoRepository>();
        services.AddSingleton<IFundingApplicationRepository, InMemoryFundingApplicationRepository>();
        services.AddSingleton<ICertificationRepository, InMemoryCertificationRepository>();
        services.AddSingleton<IVaeProjectRepository, InMemoryVaeProjectRepository>();
        services.AddSingleton<IBilanDeCompetencesRepository, InMemoryBilanDeCompetencesRepository>();
        services.AddSingleton<IElearningCourseRepository, InMemoryElearningCourseRepository>();
        services.AddSingleton<ITrainingEvaluationRepository, InMemoryTrainingEvaluationRepository>();
        services.AddSingleton<ITrainingProviderRepository, InMemoryTrainingProviderRepository>();
        services.AddSingleton<ITutoringProgramRepository, InMemoryTutoringProgramRepository>();
        services.AddSingleton<IAlternanceContractRepository, InMemoryAlternanceContractRepository>();
        services.AddSingleton<ITrainingObligationRepository, InMemoryTrainingObligationRepository>();
    }
}
