import { trainingApi } from '../apiClient';

export interface DataRetentionReportDto {
  ReportDate: string;
  TotalEmployees: number;
  AnonymizedCount: number;
  PendingAnonymizationCount: number;
  RetentionPeriodYears: number;
  OverdueCount: number;
  NextReviewDate: string;
}

export interface GdprExportDto {
  EmployeeId: string;
  ExportDate: string;
  PersonalData: Record<string, unknown>;
  TrainingHistory: unknown[];
  CompetencyHistory: unknown[];
}

const BASE_PATH = '/gdpr';

export const gdprService = {
  anonymizeEmployee: async (employeeId: string): Promise<void> => {
    await trainingApi.post(`${BASE_PATH}/anonymize/${employeeId}`);
  },

  exportEmployeeData: async (employeeId: string): Promise<GdprExportDto> => {
    const response = await trainingApi.get<GdprExportDto>(`${BASE_PATH}/export/${employeeId}`);
    return response.data;
  },

  getRetentionReport: async (): Promise<DataRetentionReportDto> => {
    const response = await trainingApi.get<DataRetentionReportDto>(`${BASE_PATH}/retention-report`);
    return response.data;
  },
};
