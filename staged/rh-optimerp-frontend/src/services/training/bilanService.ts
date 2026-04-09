import { trainingApi } from '../apiClient';
import { PagedResult } from '../../types/training';

export type BilanPhase = 'Preliminaire' | 'Investigation' | 'Conclusion';
export type BilanStatus = 'EnCours' | 'Suspendu' | 'Termine' | 'Abandonne';

export interface BilanCompetences {
  Id: string;
  EmployeeId: string;
  EmployeeNom: string;
  Phase: BilanPhase;
  Status: BilanStatus;
  StartDate: string;
  EndDate?: string;
  TotalDurationHours: number;
  UsedHours: number;
  MaxHours: number;
  OrganismeId?: string;
  OrganismeNom?: string;
}

const BASE_PATH = '/bilans-competences';

export const bilanService = {
  getById: async (id: string): Promise<BilanCompetences> => {
    const response = await trainingApi.get<BilanCompetences>(`${BASE_PATH}/${id}`);
    return response.data;
  },

  getAll: async (page = 1, pageSize = 20): Promise<PagedResult<BilanCompetences>> => {
    const response = await trainingApi.get<PagedResult<BilanCompetences>>(BASE_PATH, {
      params: { page, size: pageSize },
    });
    return response.data;
  },

  create: async (bilan: Partial<BilanCompetences>): Promise<BilanCompetences> => {
    const response = await trainingApi.post<BilanCompetences>(BASE_PATH, bilan);
    return response.data;
  },

  update: async (id: string, bilan: Partial<BilanCompetences>): Promise<BilanCompetences> => {
    const response = await trainingApi.put<BilanCompetences>(`${BASE_PATH}/${id}`, bilan);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await trainingApi.delete(`${BASE_PATH}/${id}`);
  },

  advancePhase: async (id: string): Promise<BilanCompetences> => {
    const response = await trainingApi.post<BilanCompetences>(`${BASE_PATH}/${id}/advance-phase`);
    return response.data;
  },
};
