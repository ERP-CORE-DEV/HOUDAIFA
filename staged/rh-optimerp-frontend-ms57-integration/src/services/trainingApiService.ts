import { trainingApi as api } from './apiClient';
import type {
  TrainingPlan,
  TrainingAction,
  TrainingSession,
  Competency,
  CompetencyAssessment,
  EmployeeCompetencyAssessment,
  ProfessionalInterview,
  CpfAccount,
  TrainingProvider,
  TrainingEvaluation,
  TrainingDashboardKpi,
  CertificationRncp,
  VaeProject,
  ElearningCourse,
  BilanDeCompetences,
  Opco,
  PagedResult,
} from '../types/training';

// MS 5.7 — Training & Skill Development API service
// Uses the shared trainingApi axios client from apiClient.ts.
// Base URL is /api/training (proxied to http://localhost:5207 in dev via setupProxy.js,
// routed by Nginx Ingress in production). Paths below are RELATIVE to /api/training;
// the proxy rewrites /api/training -> /api on the backend.

// ============================================================================
// Training Plans — api/training-plans
// ============================================================================

export const trainingPlanApi = {
  getAll: (page = 1, pageSize = 20) =>
    api.get<PagedResult<TrainingPlan>>('/training-plans', { params: { page, pageSize } }).then(r => r.data),

  getById: (id: string) =>
    api.get<TrainingPlan>(`/training-plans/${id}`).then(r => r.data),

  create: (dto: Partial<TrainingPlan>) =>
    api.post<TrainingPlan>('/training-plans', dto).then(r => r.data),

  update: (id: string, dto: Partial<TrainingPlan>) =>
    api.put<TrainingPlan>(`/training-plans/${id}`, dto).then(r => r.data),

  delete: (id: string) =>
    api.delete(`/training-plans/${id}`).then(r => r.data),

  approve: (id: string, approvedBy: string) =>
    api.post<TrainingPlan>(`/training-plans/${id}/approve`, { approvedBy }).then(r => r.data),
};

// ============================================================================
// Training Actions — api/training-actions
// ============================================================================

export const trainingActionApi = {
  getAll: (page = 1, pageSize = 20) =>
    api.get<PagedResult<TrainingAction>>('/training-actions', { params: { page, pageSize } }).then(r => r.data),

  getByPlan: (planId: string) =>
    api.get<TrainingAction[]>(`/training-actions/by-plan/${planId}`).then(r => r.data),

  getById: (id: string) =>
    api.get<TrainingAction>(`/training-actions/${id}`).then(r => r.data),

  create: (dto: Partial<TrainingAction>) =>
    api.post<TrainingAction>('/training-actions', dto).then(r => r.data),

  update: (id: string, dto: Partial<TrainingAction>) =>
    api.put<TrainingAction>(`/training-actions/${id}`, dto).then(r => r.data),

  delete: (id: string) =>
    api.delete(`/training-actions/${id}`).then(r => r.data),
};

// ============================================================================
// Training Sessions — api/training-sessions
// ============================================================================

export const trainingSessionApi = {
  getByAction: (actionId: string) =>
    api.get<TrainingSession[]>(`/training-sessions/by-action/${actionId}`).then(r => r.data),

  getByDateRange: (startDate: string, endDate: string) =>
    api.get<TrainingSession[]>('/training-sessions/by-date-range', { params: { startDate, endDate } }).then(r => r.data),

  getById: (id: string) =>
    api.get<TrainingSession>(`/training-sessions/${id}`).then(r => r.data),

  create: (dto: Partial<TrainingSession>) =>
    api.post<TrainingSession>('/training-sessions', dto).then(r => r.data),

  update: (id: string, dto: Partial<TrainingSession>) =>
    api.put<TrainingSession>(`/training-sessions/${id}`, dto).then(r => r.data),

  delete: (id: string) =>
    api.delete(`/training-sessions/${id}`).then(r => r.data),
};

// ============================================================================
// Competencies — api/competencies
// ============================================================================

export const competencyApi = {
  getAll: (page = 1, pageSize = 20) =>
    api.get<PagedResult<Competency>>('/competencies', { params: { page, pageSize } }).then(r => r.data),

  getById: (id: string) =>
    api.get<Competency>(`/competencies/${id}`).then(r => r.data),

  getByDomain: (domain: string) =>
    api.get<Competency[]>(`/competencies/by-domain/${domain}`).then(r => r.data),

  search: (query: string) =>
    api.get<Competency[]>('/competencies/search', { params: { query } }).then(r => r.data),

  create: (dto: Partial<Competency>) =>
    api.post<Competency>('/competencies', dto).then(r => r.data),

  update: (id: string, dto: Partial<Competency>) =>
    api.put<Competency>(`/competencies/${id}`, dto).then(r => r.data),

  delete: (id: string) =>
    api.delete(`/competencies/${id}`).then(r => r.data),
};

// ============================================================================
// Competency Assessments — api/competency-assessments
// ============================================================================

export const competencyAssessmentApi = {
  getById: (id: string) =>
    api.get<CompetencyAssessment>(`/competency-assessments/${id}`).then(r => r.data),

  getByEmployee: (employeeId: string) =>
    api.get<EmployeeCompetencyAssessment[]>(`/competency-assessments/by-employee/${employeeId}`).then(r => r.data),

  getGapAnalysis: (employeeId: string) =>
    api.get<EmployeeCompetencyAssessment[]>(`/competency-assessments/gap-analysis/${employeeId}`).then(r => r.data),

  create: (dto: Partial<CompetencyAssessment>) =>
    api.post<CompetencyAssessment>('/competency-assessments', dto).then(r => r.data),

  update: (id: string, dto: Partial<CompetencyAssessment>) =>
    api.put<CompetencyAssessment>(`/competency-assessments/${id}`, dto).then(r => r.data),
};

// ============================================================================
// Entretiens Professionnels — api/entretiens-professionnels
// ============================================================================

export const interviewApi = {
  getById: (id: string) =>
    api.get<ProfessionalInterview>(`/entretiens-professionnels/${id}`).then(r => r.data),

  getByEmployee: (employeeId: string) =>
    api.get<ProfessionalInterview[]>(`/entretiens-professionnels/by-employee/${employeeId}`).then(r => r.data),

  getOverdue: () =>
    api.get<ProfessionalInterview[]>('/entretiens-professionnels/overdue').then(r => r.data),

  create: (dto: Partial<ProfessionalInterview>) =>
    api.post<ProfessionalInterview>('/entretiens-professionnels', dto).then(r => r.data),

  update: (id: string, dto: Partial<ProfessionalInterview>) =>
    api.put<ProfessionalInterview>(`/entretiens-professionnels/${id}`, dto).then(r => r.data),

  triggerSixYearReview: (employeeId: string) =>
    api.post<ProfessionalInterview>(`/entretiens-professionnels/six-year-review/${employeeId}`, {}).then(r => r.data),
};

// ============================================================================
// CPF — api/cpf
// ============================================================================

export const cpfApi = {
  getById: (id: string) =>
    api.get<CpfAccount>(`/cpf/${id}`).then(r => r.data),

  getByEmployee: (employeeId: string) =>
    api.get<CpfAccount>(`/cpf/by-employee/${employeeId}`).then(r => r.data),

  create: (dto: Partial<CpfAccount>) =>
    api.post<CpfAccount>('/cpf', dto).then(r => r.data),

  update: (id: string, dto: Partial<CpfAccount>) =>
    api.put<CpfAccount>(`/cpf/${id}`, dto).then(r => r.data),

  creditAnnual: (id: string) =>
    api.post<CpfAccount>(`/cpf/${id}/credit-annual`, {}).then(r => r.data),

  mobilize: (id: string, amount: number, actionId: string) =>
    api.post<CpfAccount>(`/cpf/${id}/mobilize`, { amount, actionId }).then(r => r.data),
};

// ============================================================================
// Training Providers — api/training-providers
// ============================================================================

export const providerApi = {
  getAll: (page = 1, pageSize = 20) =>
    api.get<PagedResult<TrainingProvider>>('/training-providers', { params: { page, pageSize } }).then(r => r.data),

  getById: (id: string) =>
    api.get<TrainingProvider>(`/training-providers/${id}`).then(r => r.data),

  getQualiopiCertified: () =>
    api.get<TrainingProvider[]>('/training-providers/qualiopi-certified').then(r => r.data),

  create: (dto: Partial<TrainingProvider>) =>
    api.post<TrainingProvider>('/training-providers', dto).then(r => r.data),

  update: (id: string, dto: Partial<TrainingProvider>) =>
    api.put<TrainingProvider>(`/training-providers/${id}`, dto).then(r => r.data),

  delete: (id: string) =>
    api.delete(`/training-providers/${id}`).then(r => r.data),
};

// ============================================================================
// Certifications RNCP — api/certifications
// ============================================================================

export const certificationApi = {
  getAll: (page = 1, pageSize = 20) =>
    api.get<PagedResult<CertificationRncp>>('/certifications', { params: { page, pageSize } }).then(r => r.data),

  getById: (id: string) =>
    api.get<CertificationRncp>(`/certifications/${id}`).then(r => r.data),

  getExpiring: () =>
    api.get<CertificationRncp[]>('/certifications/expiring').then(r => r.data),

  create: (dto: Partial<CertificationRncp>) =>
    api.post<CertificationRncp>('/certifications', dto).then(r => r.data),

  update: (id: string, dto: Partial<CertificationRncp>) =>
    api.put<CertificationRncp>(`/certifications/${id}`, dto).then(r => r.data),

  delete: (id: string) =>
    api.delete(`/certifications/${id}`).then(r => r.data),
};

// ============================================================================
// VAE Projects — api/vae-projects
// ============================================================================

export const vaeApi = {
  getById: (id: string) =>
    api.get<VaeProject>(`/vae-projects/${id}`).then(r => r.data),

  getByEmployee: (employeeId: string) =>
    api.get<VaeProject[]>(`/vae-projects/by-employee/${employeeId}`).then(r => r.data),

  create: (dto: Partial<VaeProject>) =>
    api.post<VaeProject>('/vae-projects', dto).then(r => r.data),

  update: (id: string, dto: Partial<VaeProject>) =>
    api.put<VaeProject>(`/vae-projects/${id}`, dto).then(r => r.data),

  advancePhase: (id: string) =>
    api.post<VaeProject>(`/vae-projects/${id}/advance-phase`, {}).then(r => r.data),
};

// ============================================================================
// OPCO — api/opcos
// ============================================================================

export const opcoApi = {
  getAll: (page = 1, pageSize = 20) =>
    api.get<PagedResult<Opco>>('/opcos', { params: { page, pageSize } }).then(r => r.data),

  getById: (id: string) =>
    api.get<Opco>(`/opcos/${id}`).then(r => r.data),

  create: (dto: Partial<Opco>) =>
    api.post<Opco>('/opcos', dto).then(r => r.data),

  update: (id: string, dto: Partial<Opco>) =>
    api.put<Opco>(`/opcos/${id}`, dto).then(r => r.data),

  delete: (id: string) =>
    api.delete(`/opcos/${id}`).then(r => r.data),
};

// ============================================================================
// Funding Applications — api/funding-applications
// ============================================================================

export const fundingApi = {
  getById: (id: string) =>
    api.get(`/funding-applications/${id}`).then(r => r.data),

  create: (dto: Record<string, unknown>) =>
    api.post('/funding-applications', dto).then(r => r.data),

  update: (id: string, dto: Record<string, unknown>) =>
    api.put(`/funding-applications/${id}`, dto).then(r => r.data),

  submit: (id: string) =>
    api.post(`/funding-applications/${id}/submit`, {}).then(r => r.data),

  approve: (id: string) =>
    api.post(`/funding-applications/${id}/approve`, {}).then(r => r.data),

  reject: (id: string, reason: string) =>
    api.post(`/funding-applications/${id}/reject`, { reason }).then(r => r.data),
};

// ============================================================================
// Alternance Contracts — api/alternance-contracts
// ============================================================================

export const alternanceApi = {
  getById: (id: string) =>
    api.get(`/alternance-contracts/${id}`).then(r => r.data),

  getByEmployee: (employeeId: string) =>
    api.get(`/alternance-contracts/by-employee/${employeeId}`).then(r => r.data),

  calculateRemuneration: (params: Record<string, unknown>) =>
    api.get('/alternance-contracts/calculate-remuneration', { params }).then(r => r.data),

  create: (dto: Record<string, unknown>) =>
    api.post('/alternance-contracts', dto).then(r => r.data),

  update: (id: string, dto: Record<string, unknown>) =>
    api.put(`/alternance-contracts/${id}`, dto).then(r => r.data),

  delete: (id: string) =>
    api.delete(`/alternance-contracts/${id}`).then(r => r.data),
};

// ============================================================================
// Training Obligations (Compliance) — api/training-obligations
// ============================================================================

export const complianceApi = {
  getAll: (page = 1, pageSize = 20) =>
    api.get('/training-obligations', { params: { page, pageSize } }).then(r => r.data),

  getById: (id: string) =>
    api.get(`/training-obligations/${id}`).then(r => r.data),

  getExpiring: () =>
    api.get('/training-obligations/expiring').then(r => r.data),

  getRiskAssessment: () =>
    api.get('/training-obligations/risk-assessment').then(r => r.data),

  create: (dto: Record<string, unknown>) =>
    api.post('/training-obligations', dto).then(r => r.data),

  update: (id: string, dto: Record<string, unknown>) =>
    api.put(`/training-obligations/${id}`, dto).then(r => r.data),
};

// ============================================================================
// Bilan de Competences — api/bilans-competences
// ============================================================================

export const bilanApi = {
  getById: (id: string) =>
    api.get<BilanDeCompetences>(`/bilans-competences/${id}`).then(r => r.data),

  getByEmployee: (employeeId: string) =>
    api.get<BilanDeCompetences[]>(`/bilans-competences/by-employee/${employeeId}`).then(r => r.data),

  create: (dto: Partial<BilanDeCompetences>) =>
    api.post<BilanDeCompetences>('/bilans-competences', dto).then(r => r.data),

  update: (id: string, dto: Partial<BilanDeCompetences>) =>
    api.put<BilanDeCompetences>(`/bilans-competences/${id}`, dto).then(r => r.data),
};

// ============================================================================
// E-Learning Courses — api/elearning-courses
// ============================================================================

export const elearningApi = {
  getAll: (page = 1, pageSize = 20) =>
    api.get<PagedResult<ElearningCourse>>('/elearning-courses', { params: { page, pageSize } }).then(r => r.data),

  getById: (id: string) =>
    api.get<ElearningCourse>(`/elearning-courses/${id}`).then(r => r.data),

  create: (dto: Partial<ElearningCourse>) =>
    api.post<ElearningCourse>('/elearning-courses', dto).then(r => r.data),

  update: (id: string, dto: Partial<ElearningCourse>) =>
    api.put<ElearningCourse>(`/elearning-courses/${id}`, dto).then(r => r.data),

  delete: (id: string) =>
    api.delete(`/elearning-courses/${id}`).then(r => r.data),

  trackProgress: (courseId: string, employeeId: string, progressPercent: number) =>
    api.post(`/elearning-courses/${courseId}/track-progress`, { employeeId, progressPercent }).then(r => r.data),
};

// ============================================================================
// Training Evaluations — api/training-evaluations
// ============================================================================

export const evaluationApi = {
  getById: (id: string) =>
    api.get<TrainingEvaluation>(`/training-evaluations/${id}`).then(r => r.data),

  getBySession: (sessionId: string) =>
    api.get<TrainingEvaluation[]>(`/training-evaluations/by-session/${sessionId}`).then(r => r.data),

  getAverage: (actionId: string) =>
    api.get<number>(`/training-evaluations/average/${actionId}`).then(r => r.data),

  create: (dto: Partial<TrainingEvaluation>) =>
    api.post<TrainingEvaluation>('/training-evaluations', dto).then(r => r.data),

  update: (id: string, dto: Partial<TrainingEvaluation>) =>
    api.put<TrainingEvaluation>(`/training-evaluations/${id}`, dto).then(r => r.data),
};

// ============================================================================
// Analytics — api/training-analytics
// ============================================================================

export const analyticsApi = {
  getDashboardKpis: (year?: number) =>
    api.get<TrainingDashboardKpi>('/training-analytics/dashboard-kpis', { params: { year } }).then(r => r.data),

  getBilanSocial: () =>
    api.get('/training-analytics/bilan-social').then(r => r.data),

  getTrends: () =>
    api.get('/training-analytics/trends').then(r => r.data),

  getRoi: (trainingActionId: string) =>
    api.get(`/training-analytics/roi/${trainingActionId}`).then(r => r.data),
};

// ============================================================================
// GDPR — api/gdpr
// ============================================================================

export const gdprApi = {
  anonymize: (employeeId: string) =>
    api.post(`/gdpr/anonymize/${employeeId}`, {}).then(r => r.data),

  exportData: (employeeId: string) =>
    api.get(`/gdpr/export/${employeeId}`).then(r => r.data),

  getRetentionStatus: () =>
    api.get('/gdpr/retention-status').then(r => r.data),
};
