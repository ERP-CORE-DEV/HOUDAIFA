using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Training.SkillDevelopment.Configuration;
using Training.SkillDevelopment.Extensions;
using Training.SkillDevelopment.Repositories.FormationExecution.TrainingPlan;
using Training.SkillDevelopment.Repositories.FormationExecution.TrainingAction;
using Training.SkillDevelopment.Repositories.FinancementConformite.Cpf;
using Training.SkillDevelopment.Repositories.Competency;
using Training.SkillDevelopment.Repositories.EntretienProfessionnel;
using Training.SkillDevelopment.Repositories.FinancementConformite.Opco;
using Training.SkillDevelopment.Repositories.CertificationEcosysteme.Certification;
using Training.SkillDevelopment.Repositories.BilanCompetences;
using Training.SkillDevelopment.Repositories.FormationExecution.ELearning;
using Training.SkillDevelopment.Repositories.FormationExecution.Evaluation;
using Training.SkillDevelopment.Repositories.CertificationEcosysteme.Provider;
using Training.SkillDevelopment.Repositories.Tutorat;
using Training.SkillDevelopment.Repositories.FinancementConformite.Alternance;
using Training.SkillDevelopment.Repositories.FinancementConformite.Compliance;
using Training.SkillDevelopment.Services.FormationExecution.TrainingPlan;
using Training.SkillDevelopment.Services.FormationExecution.TrainingAction;
using Training.SkillDevelopment.Services.FinancementConformite.Cpf;
using Training.SkillDevelopment.Services.Competency;
using Training.SkillDevelopment.Services.EntretienProfessionnel;
using Training.SkillDevelopment.Services.FinancementConformite.Opco;
using Training.SkillDevelopment.Services.CertificationEcosysteme.Certification;
using Training.SkillDevelopment.Services.BilanCompetences;
using Training.SkillDevelopment.Services.FormationExecution.ELearning;
using Training.SkillDevelopment.Services.FormationExecution.Evaluation;
using Training.SkillDevelopment.Services.CertificationEcosysteme.Provider;
using Training.SkillDevelopment.Services.Tutorat;
using Training.SkillDevelopment.Services.FinancementConformite.Alternance;
using Training.SkillDevelopment.Services.FinancementConformite.Compliance;
using Training.SkillDevelopment.Services.PilotageGouvernance.Analytics;
using Training.SkillDevelopment.Services.CareerGuidance;
using Training.SkillDevelopment.Services.PilotageGouvernance.Gdpr;

var builder = WebApplication.CreateBuilder(args);

// --- CORS ---
var frontendOrigins = builder.Environment.IsDevelopment()
    ? new[] { "http://localhost:3000", "https://localhost:3000" }
    : new[] { builder.Configuration["Frontend:ProductionUrl"] ?? "https://rh-optimerp-training.azurewebsites.net" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(frontendOrigins)
            .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
            .WithHeaders("Content-Type", "Authorization")
            .AllowCredentials();
    });
});

// --- Logging ---
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.Configure<LoggerFilterOptions>(options =>
{
    options.MinLevel = builder.Environment.IsDevelopment()
        ? LogLevel.Debug
        : LogLevel.Information;
});

// --- Configuration ---
builder.Services.Configure<CosmosDbSettings>(
    builder.Configuration.GetSection(CosmosDbSettings.SectionName));
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));

// --- JWT Authentication (shared symmetric key per team convention, see rh-optimerp-frontend#2) ---
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");

if (string.IsNullOrWhiteSpace(jwtSettings.Key))
    throw new InvalidOperationException("Jwt:Key is not configured.");

var jwtSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = jwtSigningKey,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// --- Global exception handler (ProblemDetails per team convention) ---
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// --- Controllers & JSON ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Training & Skill Development API",
        Version = "v1.0.0",
        Description = "MS 5.7 — Formation, CPF, Competences, Entretiens — RH-OptimERP"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Saisissez votre token JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

// --- CosmosDB ---
CosmosClient? cosmosClient = null;
var useInMemory = false;

try
{
    var cosmosConnectionString = builder.Configuration.GetConnectionString("CosmosDb");
    if (string.IsNullOrEmpty(cosmosConnectionString))
        throw new InvalidOperationException("CosmosDB connection string not configured");

    cosmosClient = new CosmosClient(cosmosConnectionString, new CosmosClientOptions
    {
        ConnectionMode = ConnectionMode.Direct,
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.Default
        }
    });
    builder.Services.AddSingleton(cosmosClient);
    Console.WriteLine("[OK] CosmosDB client initialized (direct mode)");
}
catch (Exception ex)
{
    Console.WriteLine($"[WARN] CosmosDB init failed: {ex.Message}");
    if (builder.Environment.IsDevelopment())
    {
        useInMemory = true;
        Console.WriteLine("[INFO] Using InMemory repositories (Development)");
    }
    else
    {
        throw new InvalidOperationException(
            "CosmosDB is required in Production. Set ConnectionStrings:CosmosDb.", ex);
    }
}

// --- Repository Registration ---
if (useInMemory)
{
    builder.Services.AddSingleton<ITrainingPlanRepository, InMemoryTrainingPlanRepository>();
    builder.Services.AddSingleton<ITrainingActionRepository, InMemoryTrainingActionRepository>();
    builder.Services.AddSingleton<ITrainingSessionRepository, InMemoryTrainingSessionRepository>();
    builder.Services.AddSingleton<IEnrollmentRepository, InMemoryEnrollmentRepository>();
    builder.Services.AddSingleton<ICpfAccountRepository, InMemoryCpfAccountRepository>();
    builder.Services.AddSingleton<ICompetencyRepository, InMemoryCompetencyRepository>();
    builder.Services.AddSingleton<ICompetencyAssessmentRepository, InMemoryCompetencyAssessmentRepository>();
    builder.Services.AddSingleton<IProfessionalInterviewRepository, InMemoryProfessionalInterviewRepository>();
    builder.Services.AddSingleton<IOpcoRepository, InMemoryOpcoRepository>();
    builder.Services.AddSingleton<IFundingApplicationRepository, InMemoryFundingApplicationRepository>();
    builder.Services.AddSingleton<ICertificationRepository, InMemoryCertificationRepository>();
    builder.Services.AddSingleton<IVaeProjectRepository, InMemoryVaeProjectRepository>();
    builder.Services.AddSingleton<IBilanDeCompetencesRepository, InMemoryBilanDeCompetencesRepository>();
    builder.Services.AddSingleton<IElearningCourseRepository, InMemoryElearningCourseRepository>();
    builder.Services.AddSingleton<ITrainingEvaluationRepository, InMemoryTrainingEvaluationRepository>();
    builder.Services.AddSingleton<ITrainingProviderRepository, InMemoryTrainingProviderRepository>();
    builder.Services.AddSingleton<ITutoringProgramRepository, InMemoryTutoringProgramRepository>();
    builder.Services.AddSingleton<IAlternanceContractRepository, InMemoryAlternanceContractRepository>();
    builder.Services.AddSingleton<ITrainingObligationRepository, InMemoryTrainingObligationRepository>();
}
else
{
    builder.Services.AddScoped<ITrainingPlanRepository, CosmosTrainingPlanRepository>();
    builder.Services.AddScoped<ITrainingActionRepository, CosmosTrainingActionRepository>();
    builder.Services.AddScoped<ITrainingSessionRepository, CosmosTrainingSessionRepository>();
    builder.Services.AddScoped<IEnrollmentRepository, CosmosEnrollmentRepository>();
    builder.Services.AddScoped<ICpfAccountRepository, CosmosCpfAccountRepository>();
    builder.Services.AddScoped<ICompetencyRepository, CosmosCompetencyRepository>();
    builder.Services.AddScoped<ICompetencyAssessmentRepository, CosmosCompetencyAssessmentRepository>();
    builder.Services.AddScoped<IProfessionalInterviewRepository, CosmosProfessionalInterviewRepository>();
    builder.Services.AddScoped<IOpcoRepository, CosmosOpcoRepository>();
    builder.Services.AddScoped<IFundingApplicationRepository, CosmosFundingApplicationRepository>();
    builder.Services.AddScoped<ICertificationRepository, CosmosCertificationRepository>();
    builder.Services.AddScoped<IVaeProjectRepository, CosmosVaeProjectRepository>();
    builder.Services.AddScoped<IBilanDeCompetencesRepository, CosmosBilanDeCompetencesRepository>();
    builder.Services.AddScoped<IElearningCourseRepository, CosmosElearningCourseRepository>();
    builder.Services.AddScoped<ITrainingEvaluationRepository, CosmosTrainingEvaluationRepository>();
    builder.Services.AddScoped<ITrainingProviderRepository, CosmosTrainingProviderRepository>();
    builder.Services.AddScoped<ITutoringProgramRepository, CosmosTutoringProgramRepository>();
    builder.Services.AddScoped<IAlternanceContractRepository, CosmosAlternanceContractRepository>();
    builder.Services.AddScoped<ITrainingObligationRepository, CosmosTrainingObligationRepository>();
}

// --- Service Registration ---
builder.Services.AddScoped<ITrainingPlanService, TrainingPlanService>();
builder.Services.AddScoped<ITrainingActionService, TrainingActionService>();
builder.Services.AddScoped<ITrainingSessionService, TrainingSessionService>();
builder.Services.AddScoped<ICpfService, CpfService>();
builder.Services.AddScoped<ICompetencyService, CompetencyService>();
builder.Services.AddScoped<ICompetencyAssessmentService, CompetencyAssessmentService>();
builder.Services.AddScoped<IProfessionalInterviewService, ProfessionalInterviewService>();
builder.Services.AddScoped<IOpcoService, OpcoService>();
builder.Services.AddScoped<IFundingApplicationService, FundingApplicationService>();
builder.Services.AddScoped<ICertificationService, CertificationService>();
builder.Services.AddScoped<IVaeProjectService, VaeProjectService>();
builder.Services.AddScoped<IBilanService, BilanService>();
builder.Services.AddScoped<IElearningService, ElearningService>();
builder.Services.AddScoped<ITrainingEvaluationService, TrainingEvaluationService>();
builder.Services.AddScoped<ITrainingProviderService, TrainingProviderService>();
builder.Services.AddScoped<ITutoringService, TutoringService>();
builder.Services.AddScoped<IAlternanceService, AlternanceService>();
builder.Services.AddScoped<ITrainingObligationService, TrainingObligationService>();
builder.Services.AddScoped<ITrainingAnalyticsService, TrainingAnalyticsService>();
builder.Services.AddScoped<ICareerGuidanceService, CareerGuidanceService>();
builder.Services.AddScoped<IGdprService, GdprService>();

// --- FluentValidation ---
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// --- Health Checks ---
builder.Services.AddHealthChecks()
    .AddCheck("api-ready", () => HealthCheckResult.Healthy("API is ready"))
    .AddCheck("cosmosdb", () =>
    {
        if (cosmosClient != null)
        {
            try
            {
                _ = cosmosClient.Endpoint;
                return HealthCheckResult.Healthy("CosmosDB connected");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Degraded("CosmosDB validation failed", ex);
            }
        }
        return useInMemory
            ? HealthCheckResult.Healthy("InMemory mode (Development)")
            : HealthCheckResult.Unhealthy("CosmosDB not configured");
    });

var app = builder.Build();

// --- Middleware ---
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("FrontendPolicy");
app.UseStaticFiles();

// Security headers — applied in ALL environments
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
