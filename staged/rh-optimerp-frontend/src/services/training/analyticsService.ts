import { trainingApi } from '../apiClient';
import { TrainingDashboardKpi } from '../../types/training';

const BASE_PATH = '/training-analytics';

export const analyticsService = {
  getDashboardKpis: async (year: number): Promise<TrainingDashboardKpi> => {
    const response = await trainingApi.get<TrainingDashboardKpi>(`${BASE_PATH}/dashboard/${year}`);
    return response.data;
  },

  getBilanSocial: async (year: number): Promise<Record<string, unknown>> => {
    const response = await trainingApi.get(`${BASE_PATH}/bilan-social/${year}`);
    return response.data;
  },

  getGenderEquality: async (year: number): Promise<Record<string, unknown>> => {
    const response = await trainingApi.get(`${BASE_PATH}/gender-equality/${year}`);
    return response.data;
  },

  getTrainingRoi: async (actionId: string): Promise<number> => {
    const response = await trainingApi.get<number>(`${BASE_PATH}/roi/${actionId}`);
    return response.data;
  },
};
