import { trainingApi } from '../apiClient';
import { PagedResult, EvaluationLevel } from '../../types/training';

export interface EvaluationRecord {
  Id: string;
  SessionId: string;
  SessionTitle: string;
  EvaluateurId: string;
  EvaluateurNom: string;
  Level: EvaluationLevel;
  Score: number;
  EvaluationDate: string;
  Comments?: string;
  IsActive: boolean;
}

const BASE_PATH = '/evaluations';

export const evaluationService = {
  getById: async (id: string): Promise<EvaluationRecord> => {
    const response = await trainingApi.get<EvaluationRecord>(`${BASE_PATH}/${id}`);
    return response.data;
  },

  getAll: async (page = 1, pageSize = 20): Promise<PagedResult<EvaluationRecord>> => {
    const response = await trainingApi.get<PagedResult<EvaluationRecord>>(BASE_PATH, {
      params: { page, size: pageSize },
    });
    return response.data;
  },

  create: async (evaluation: Partial<EvaluationRecord>): Promise<EvaluationRecord> => {
    const response = await trainingApi.post<EvaluationRecord>(BASE_PATH, evaluation);
    return response.data;
  },

  update: async (id: string, evaluation: Partial<EvaluationRecord>): Promise<EvaluationRecord> => {
    const response = await trainingApi.put<EvaluationRecord>(`${BASE_PATH}/${id}`, evaluation);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await trainingApi.delete(`${BASE_PATH}/${id}`);
  },
};
