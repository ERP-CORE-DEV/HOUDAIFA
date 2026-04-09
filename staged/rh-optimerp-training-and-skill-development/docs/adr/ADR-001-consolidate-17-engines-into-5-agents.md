# ADR-001: Consolidate 17 Sub-Modules into 5 Engines

**Status:** Accepted
**Date:** 2026-03-17
**Deciders:** Houdaifa Attazrouti (MS 5.7 Lead)
**Scope:** MS 5.7 — Training & Skill Development

---

## Context

MS 5.7 was decomposed during brainstorming into **17 sub-modules** with **220 sub-features** covering all aspects of French training and skill development law. This granularity was necessary for feature completeness and legal coverage, producing:

- 22 controllers, 21 services, 19 repositories, 50+ entities, 18 validators
- 515 tests (433 backend + 82 frontend)
- 16 frontend pages, 7 CRUD modals

However, the DAT document describes MS 5.7 at a higher level as a single microservice that *"coordinates employee training programs, skill assessments, and reporting"*. The 17-sub-module granularity creates:

1. **Cognitive overhead** — developers must navigate 17 separate feature folders
2. **Cross-module coupling** — training plans (M1) reference training actions (M2), which reference evaluations (M10) and e-learning (M9)
3. **API surface sprawl** — 22 controllers with overlapping domain boundaries
4. **Deployment complexity** — all 17 sub-modules deploy as one unit anyway (single microservice)
5. **International readiness** — grouping France-specific sub-modules isolates regulatory logic for future country adapters

## Decision

Consolidate the 17 sub-modules into **5 engines** within the same microservice. Each engine is a **logical grouping** (namespace + folder boundary), not a separate deployable. The 17 sub-modules remain as internal modules within their parent engine.

### Engine 1: Formation & Execution (Training Lifecycle)

**Responsibility:** Plan-Do-Check-Act cycle for all training delivery

| Module | Sub-features | Controllers |
|--------|-------------|-------------|
| M1: Plan de Developpement des Competences | 14 | TrainingPlanController |
| M2: Action de Formation | 16 | TrainingActionController, TrainingSessionController |
| M9: E-Learning & Blended Learning | 15 | ElearningController |
| M10: Evaluation de la Formation | 15 | TrainingEvaluationController |
| **Total** | **60** | **5 controllers** |

**Domain events emitted:** TrainingPlanApproved, SessionCompleted, ElearningProgressUpdated, EvaluationSubmitted
**Sync with MS 5.8:** TrainingCompleted (outbound)

**Cohesion rationale:**
- Training plans (M1) define *what* training to deliver
- Training actions (M2) and e-learning (M9) *execute* that delivery
- Evaluations (M10) measure *effectiveness* of the delivery
- These 4 modules form a closed PDCA loop with tight data dependencies

**Key entities:** TrainingPlan, TrainingPlanLine, TrainingBudget, TrainingAction, TrainingSession, Enrollment, AttendanceRecord, Trainer, TrainingCatalogEntry, TrainingPath, ElearningCourse, LearningPath, LearnerProgress, TrainingEvaluation, EvaluationQuestionnaire

---

### Engine 2: Competences & Parcours Professionnel (Skills & Career Development)

**Responsibility:** Assess, track, and develop employee competencies and career paths

| Module | Sub-features | Controllers |
|--------|-------------|-------------|
| M4: Referentiel de Competences | 14 | CompetencyController, CompetencyAssessmentController |
| M5: Entretien Professionnel | 14 | EntretienProfessionnelController |
| M8: Bilan de Competences | 11 | BilanCompetencesController |
| M12: Tutorat & Mentorat | 12 | TutoratController |
| M16: Conseil en Evolution Professionnelle | 8 | CareerGuidanceController |
| **Total** | **59** | **6 controllers** |

**Domain events emitted:** CompetencyAssessed, EntretienCompleted, BilanCompleted, CareerProjectCreated
**Sync with MS 5.8:** CompetencyCreated/Updated (outbound), SkillProfileUpdated (inbound), SkillGapIdentified (bidirectional), EntretienCompleted (outbound), CareerProjectCreated (inbound from 5.8), GpecNeedsUpdated (inbound)

**Cohesion rationale:**
- Competency framework (M4) defines the *skills dictionary* used by all other modules here
- Professional interviews (M5) *assess* employees against the competency framework
- Bilan de competences (M8) provides *deep skills review* feeding career decisions
- Tutoring/mentoring (M12) is a *development mechanism* for closing competency gaps
- Career guidance (M16) *recommends paths* based on competency assessments
- This engine is the **primary sync partner with MS 5.8** (Talent Management)

**Key entities:** Competency, CompetencyLevel, CompetencyDomain, CompetencyGap, EmployeeCompetencyAssessment, ProfessionalInterview, InterviewReport, SixYearReview, InterviewObligation, BilanDeCompetences, TutoringProgram, MaitreApprentissage, TutorDesignation, CareerProject, TrainingRecommendation

---

### Engine 3: Financement & Conformite Reglementaire (Funding & Regulatory Compliance)

**Responsibility:** French labor law compliance, funding mechanisms, and apprenticeship contracts

| Module | Sub-features | Controllers |
|--------|-------------|-------------|
| M3: CPF (Compte Personnel de Formation) | 13 | CpfController |
| M6: OPCO & Financement | 14 | OpcoController, FundingController |
| M13: Alternance | 12 | AlternanceController |
| M14: Obligation de Formation & Conformite | 13 | ComplianceController |
| **Total** | **52** | **5 controllers** |

**Domain events emitted:** CpfMobilized, FundingApproved, AlternanceContractCreated, ComplianceAlertTriggered

**Cohesion rationale:**
- All 4 modules are **100% France-specific** regulatory mechanisms
- CPF (M3) and OPCO funding (M6) are the two *funding sources* for training
- Alternance (M13) has its own *funding and compliance* requirements (CERFA, SMIC grids, subsidies)
- Compliance obligations (M14) *enforce* mandatory training deadlines across all funding types
- **International isolation:** this entire engine would be replaced by a country-specific adapter for non-French deployments

**Key entities:** CpfAccount, CpfTransaction, CpfMobilization, Opco, FundingApplication, FundingCriteria, TrainingContribution, FundingEnvelope, ClauseDeditFormation, AlternanceContract, AlternanceRemuneration, TrainingObligation, EmployeeObligationStatus, ComplianceAlert

---

### Engine 4: Certification & Ecosysteme (Credentials & Provider Network)

**Responsibility:** Manage certifications, VAE processes, and external training provider relationships

| Module | Sub-features | Controllers |
|--------|-------------|-------------|
| M7: Certification & VAE | 14 | CertificationController, VaeController |
| M11: Organisme de Formation & Fournisseur | 11 | TrainingProviderController |
| **Total** | **25** | **3 controllers** |

**Domain events emitted:** CertificationObtained, VaeCompleted, ProviderQualiopiVerified
**Sync with MS 5.8:** CertificationObtained (outbound)

**Cohesion rationale:**
- Certifications (M7) are *delivered by* training providers (M11)
- Provider quality (Qualiopi) directly impacts *certification validity*
- VAE projects require a *certified provider* (organisme agree)
- Both modules manage the *external ecosystem* of the training function
- Smaller engine by design — certifications are high-value, low-volume operations

**Key entities:** CertificationRncp, CertificationRs, EmployeeCertification, VaeProject, RegulatoryHabilitation, BlockOfCompetency, TrainingProvider, TrainingConvention, ProviderEvaluation, ProviderInvoice, ProviderContract

---

### Engine 5: Pilotage & Gouvernance (Analytics, Reporting & Data Governance)

**Responsibility:** Cross-cutting dashboards, regulatory reporting, and GDPR compliance

| Module | Sub-features | Controllers |
|--------|-------------|-------------|
| M15: Analytics & Reporting Formation | 14 | AnalyticsController |
| M17: GDPR & Data Protection Formation | 10 | GdprController |
| **Total** | **24** | **2 controllers** |

**Domain events consumed:** All domain events from Engines 1-4 (read model)

**Cohesion rationale:**
- Analytics (M15) *reads data from all other engines* — it's a cross-cutting concern
- GDPR (M17) *governs data lifecycle across all engines* (anonymization, retention, portability)
- Both modules have **no domain writes of their own** — they read, aggregate, and report
- Smaller controller count is correct: these are *query-heavy, command-light* modules
- GDPR operations (anonymize, export, delete) touch entities from all 4 other engines

**Key entities:** TrainingDashboard, TrainingKpi, BilanSocialTraining, GenderEqualityTrainingReport, CustomReport, TrainingTrend, GdprConsent, DataRetentionPolicy, DataAccessLog, AnonymizationRecord, DataProcessingRegister

---

## Architecture Diagram

```
MS 5.7 — Training & Skill Development
+-----------------------------------------------------------------------+
|                                                                       |
|  +---------------------------+    +-------------------------------+   |
|  | E1: Formation & Execution |    | E2: Competences & Parcours    |   |
|  |---------------------------|    |-------------------------------|   |
|  | M1  Training Plan    (14) |    | M4  Competency Framework (14) |   |
|  | M2  Training Action  (16) |<-->| M5  Entretien Prof.      (14) |   |
|  | M9  E-Learning       (15) |    | M8  Bilan Competences    (11) |   |
|  | M10 Evaluation       (15) |    | M12 Tutorat & Mentorat   (12) |   |
|  |                           |    | M16 Career Guidance       (8) |   |
|  | 60 sub-features | 5 ctrl  |    | 59 sub-features | 6 ctrl     |   |
|  +---------------------------+    +-------------------------------+   |
|          |         |                       |          |               |
|          v         v                       v          v               |
|  +---------------------------+    +-------------------------------+   |
|  | E3: Financement &         |    | E4: Certification &           |   |
|  |     Conformite            |    |     Ecosysteme                |   |
|  |---------------------------|    |-------------------------------|   |
|  | M3  CPF              (13) |    | M7  Certification & VAE (14)  |   |
|  | M6  OPCO & Funding   (14) |    | M11 Provider Management (11)  |   |
|  | M13 Alternance       (12) |    |                               |   |
|  | M14 Compliance       (13) |    | 25 sub-features | 3 ctrl      |   |
|  |                           |    +-------------------------------+   |
|  | 52 sub-features | 5 ctrl  |                                       |
|  +---------------------------+                                       |
|                                                                       |
|  +-------------------------------------------------------------------+
|  | E5: Pilotage & Gouvernance (Cross-Cutting)                        |
|  |-----------------------------------------------------------------  |
|  | M15 Analytics & Reporting (14)  |  M17 GDPR & Data Protection (10)|
|  | 24 sub-features | 2 ctrl       |  Reads from ALL engines         |
|  +-------------------------------------------------------------------+
|                                                                       |
+----------------------------+------------------------------------------+
                             |
                     Azure Service Bus
                             |
              +--------------+--------------+
              |                             |
     MS 5.6 Retention              MS 5.8 Talent Mgmt
       (upstream)                  (MOHAMMED-REDA)
```

## Consequences

### Positive
- **Reduced cognitive load:** 5 engines vs 17 sub-modules as top-level navigation
- **Domain-aligned boundaries:** each engine has a clear single responsibility
- **International readiness:** Engine 3 (Financement & Conformite) is fully isolatable for country adapters
- **MS 5.8 sync clarity:** Engine 2 owns the majority of the bidirectional sync contract
- **Cross-cutting isolation:** Engine 5 cleanly separates read-model analytics from write-model operations
- **No code deletion:** all 17 sub-modules, 220 sub-features, and 515 tests are preserved

### Negative
- **Engine size imbalance:** E1 (60 sub-features) vs E4 (25 sub-features) — accepted because domain cohesion matters more than equal sizing
- **Cross-engine references:** training plans (E1) may reference competency gaps (E2) and funding (E3) — mitigated by internal service injection within the same microservice
- **Migration effort:** namespace restructuring across 22 controllers, 21 services, 19 repositories

### Neutral
- **No deployment change:** all 5 engines still deploy as a single microservice on AKS
- **No API change:** all existing endpoints remain identical
- **No database change:** CosmosDB containers and partition keys are unchanged

## Alternatives Considered

### 1. Keep 17 sub-modules flat (Status Quo)
- Rejected: too granular for team navigation, no clear ownership boundaries

### 2. Consolidate into 3 engines (Training / Compliance / Analytics)
- Rejected: merges competency management with training execution, losing the natural skill assessment boundary

### 3. Consolidate into 7 engines (one per DAT entity)
- Rejected: the DAT only defines 7 entities (Employee, TrainingEvent, RoleChangeEvent, EventBus, DataLake, DataWarehouse, AnalyticsReport) which are too coarse for meaningful grouping

### 4. Split into separate microservices
- Rejected: violates the DAT's single-microservice boundary for MS 5.7; would require API gateway changes and distributed transaction handling

## Implementation Plan

### Phase 1: Namespace restructuring (logical only)
- Create 5 engine folders under `Controllers/`, `Services/`, `Repositories/`
- Move existing files into engine subfolders
- Update DI registrations in `Program.cs`
- Update namespace declarations

### Phase 2: Frontend alignment
- Group sidebar menu items by engine
- Update route prefixes if desired (optional — current routes work)

### Phase 3: Documentation
- Update brainstorming.md with engine mapping
- Update team onboarding docs

## References

- [DAT Document Section 5.7](C:\Users\Houdaifa\Downloads\DAT_document_for_RH-OptimERP_Final_2.txt)
- [Engine Brainstorming](.claude/engine-artifacts/brainstorming.md)
- [Session 7 Checkpoint](.claude/checkpoints/SESSION_7_CHECKPOINT.json)
