export interface PagedResult<T> {
  Items: T[];
  TotalCount: number;
  Page: number;
  PageSize: number;
  TotalPages: number;
  HasNextPage: boolean;
  HasPreviousPage: boolean;
}

export type TrainingPlanStatus =
  | 'Draft'
  | 'PendingApproval'
  | 'Approved'
  | 'InExecution'
  | 'Completed'
  | 'Archived';

export type TrainingActionType = 'Obligatoire' | 'Adaptation' | 'Developpement';
export type TrainingModality = 'Presentiel' | 'Distanciel' | 'ELearning' | 'Blended' | 'Afest';
export type SessionStatus = 'Planned' | 'Confirmed' | 'InProgress' | 'Completed' | 'Cancelled' | 'Postponed';
export type EnrollmentStatus = 'Requested' | 'Validated' | 'Convoked' | 'Confirmed' | 'Attended' | 'Absent' | 'Cancelled';
export type CompetencyLevel = 'Initie' | 'Pratique' | 'Confirme' | 'Expert' | 'Referent';
export type InterviewType = 'Biennial' | 'SixYearReview' | 'PostAbsence' | 'Voluntary';
export type VaeStatus = 'Recevabilite' | 'Accompagnement' | 'Livret2' | 'Jury' | 'ValidationTotale' | 'ValidationPartielle' | 'Refus';
export type FundingStatus = 'Draft' | 'Submitted' | 'UnderReview' | 'Approved' | 'Rejected' | 'Paid';
export type EvaluationLevel = 'Satisfaction' | 'Apprentissage' | 'Transfert' | 'Resultats';

export interface TrainingPlan {
  Id: string;
  CompanyId: string;
  Title: string;
  Year: number;
  Status: TrainingPlanStatus;
  BudgetAllocated: number;
  BudgetConsumed: number;
  MasseSalariale: number;
  Description?: string;
  IsActive: boolean;
  CreatedAt: string;
  UpdatedAt: string;
}

export interface TrainingAction {
  Id: string;
  PlanId?: string;
  Title: string;
  Description?: string;
  Type: TrainingActionType;
  Modality: TrainingModality;
  DurationHours: number;
  Cost: number;
  MaxParticipants: number;
  IsObligatory: boolean;
  IsActive: boolean;
}

export interface TrainingSession {
  Id: string;
  ActionId: string;
  StartDate: string;
  EndDate: string;
  Location?: string;
  Status: SessionStatus;
  EnrolledCount: number;
  MaxCapacity: number;
}

export interface Competency {
  Id: string;
  Code: string;
  Name: string;
  Description?: string;
  Domain: string;
  Family: string;
  Type: string;
  IsCritical: boolean;
  IsActive: boolean;
}

export interface CompetencyAssessment {
  Id: string;
  EmployeeId: string;
  CompetencyId: string;
  CurrentLevel: CompetencyLevel;
  AssessedBy: string;
  AssessmentDate: string;
}

export interface ProfessionalInterview {
  Id: string;
  EmployeeId: string;
  ManagerId: string;
  Type: InterviewType;
  ScheduledDate: string;
  ConductedDate?: string;
  Status: string;
}

export interface CpfAccount {
  Id: string;
  EmployeeId: string;
  BalanceEuros: number;
  CeilingEuros: number;
  AnnualCreditEuros: number;
  IsLowQualified: boolean;
}

export interface TrainingProvider {
  Id: string;
  Name: string;
  NdaNumber?: string;
  QualiopiCertified: boolean;
  QualiopiExpirationDate?: string;
  Rating?: number;
  IsActive: boolean;
}

export interface TrainingEvaluation {
  Id: string;
  TrainingActionId: string;
  SessionId: string;
  EmployeeId: string;
  EvaluationType: string;
  OverallScore: number;
  Comments?: string;
}

export interface TrainingDashboardKpi {
  Year: number;
  TotalTrainingPlans: number;
  TotalTrainingActions: number;
  TotalSessions: number;
  TotalEnrollments: number;
  TotalBudgetAllocated: number;
  TotalBudgetConsumed: number;
  BudgetUtilizationRate: number;
  AverageSatisfactionScore: number;
  CompletionRate: number;
}

export interface EmployeeCompetencyAssessment {
  Id: string;
  EmployeeId: string;
  CompetencyId: string;
  CompetencyName: string;
  CompetencyCode: string;
  CurrentLevel: CompetencyLevel;
  TargetLevel?: CompetencyLevel;
  AssessedBy: string;
  AssessmentDate: string;
  Comments?: string;
}

export interface CertificationRncp {
  Id: string;
  RncpCode: string;
  Titre: string;
  Organisme: string;
  NiveauQualification: string;
  DateEnregistrement: string;
  DateEcheance?: string;
  IsActive: boolean;
}

export interface VaeProject {
  Id: string;
  EmployeeId: string;
  CertificationId: string;
  CertificationTitre?: string;
  Status: VaeStatus;
  Phase: string;
  StartDate: string;
  ExpectedEndDate?: string;
  ActualEndDate?: string;
  Comments?: string;
}

export type ElearningFormat = 'SCORM' | 'xAPI' | 'Video' | 'Interactive' | 'Assessment';

export interface ElearningCourse {
  Id: string;
  Title: string;
  Description?: string;
  Format: ElearningFormat;
  DurationMinutes: number;
  Url?: string;
  IsActive: boolean;
  IsMandatory: boolean;
}

export type BilanStatus = 'Requested' | 'InProgress' | 'Completed' | 'Cancelled';

export interface BilanDeCompetences {
  Id: string;
  EmployeeId: string;
  Status: BilanStatus;
  StartDate: string;
  EndDate?: string;
  ProviderName?: string;
  FundingSource?: string;
  Comments?: string;
}

export interface Opco {
  Id: string;
  Code: string;
  Name: string;
  ContactEmail?: string;
  ContributionRate: number;
  BranchesProfessionnelles?: string;
  IsActive: boolean;
}

export type GdprRequestType = 'Access' | 'Rectification' | 'Deletion' | 'Portability' | 'Objection';
export type GdprRequestStatus = 'Pending' | 'InProgress' | 'Completed' | 'Rejected';

export interface GdprRequest {
  Id: string;
  EmployeeId: string;
  RequestType: GdprRequestType;
  Status: GdprRequestStatus;
  RequestDate: string;
  CompletedDate?: string;
  Notes?: string;
}
