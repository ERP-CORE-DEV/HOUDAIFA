import { trainingApi } from '../apiClient';
import { PagedResult } from '../../types/training';

export type ComplianceStatus = 'Conforme' | 'NonConforme' | 'EnCours';
export type RiskLevel = 'Faible' | 'Moyen' | 'Eleve' | 'Critique';
export type ObligationType = 'Reglementaire' | 'Conventionnelle' | 'Interne';

export interface ComplianceObligation {
  Id: string;
  Title: string;
  ObligationType: ObligationType;
  Echeance: string;
  Status: ComplianceStatus;
  RiskLevel: RiskLevel;
  Description?: string;
  ResponsableId?: string;
  ResponsableNom?: string;
  LastUpdated: string;
  IsActive: boolean;
}

const BASE_PATH = '/compliance-obligations';

export const complianceService = {
  getById: async (id: string): Promise<ComplianceObligation> => {
    const response = await trainingApi.get<ComplianceObligation>(`${BASE_PATH}/${id}`);
    return response.data;
  },

  getAll: async (page = 1, pageSize = 20): Promise<PagedResult<ComplianceObligation>> => {
    const response = await trainingApi.get<PagedResult<ComplianceObligation>>(BASE_PATH, {
      params: { page, size: pageSize },
    });
    return response.data;
  },

  create: async (obligation: Partial<ComplianceObligation>): Promise<ComplianceObligation> => {
    const response = await trainingApi.post<ComplianceObligation>(BASE_PATH, obligation);
    return response.data;
  },

  update: async (id: string, obligation: Partial<ComplianceObligation>): Promise<ComplianceObligation> => {
    const response = await trainingApi.put<ComplianceObligation>(`${BASE_PATH}/${id}`, obligation);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await trainingApi.delete(`${BASE_PATH}/${id}`);
  },

  markConforme: async (id: string): Promise<ComplianceObligation> => {
    const response = await trainingApi.post<ComplianceObligation>(`${BASE_PATH}/${id}/mark-conforme`);
    return response.data;
  },
};
