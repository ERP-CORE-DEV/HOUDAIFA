import { trainingApi } from '../apiClient';
import { TrainingPlan, PagedResult } from '../../types/training';

const BASE_PATH = '/training-plans';

export const trainingPlanService = {
  getById: async (id: string): Promise<TrainingPlan> => {
    const response = await trainingApi.get<TrainingPlan>(`${BASE_PATH}/${id}`);
    return response.data;
  },

  getAll: async (page = 1, pageSize = 20): Promise<PagedResult<TrainingPlan>> => {
    const response = await trainingApi.get<PagedResult<TrainingPlan>>(BASE_PATH, {
      params: { page, size: pageSize },
    });
    return response.data;
  },

  create: async (plan: Partial<TrainingPlan>): Promise<TrainingPlan> => {
    const response = await trainingApi.post<TrainingPlan>(BASE_PATH, plan);
    return response.data;
  },

  update: async (id: string, plan: Partial<TrainingPlan>): Promise<TrainingPlan> => {
    const response = await trainingApi.put<TrainingPlan>(`${BASE_PATH}/${id}`, plan);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await trainingApi.delete(`${BASE_PATH}/${id}`);
  },

  approve: async (id: string): Promise<TrainingPlan> => {
    const response = await trainingApi.post<TrainingPlan>(`${BASE_PATH}/${id}/approve`);
    return response.data;
  },
};
