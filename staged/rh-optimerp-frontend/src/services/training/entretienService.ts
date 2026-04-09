import { trainingApi } from '../apiClient';
import { ProfessionalInterview, PagedResult } from '../../types/training';

const BASE_PATH = '/professional-interviews';

export const entretienService = {
  getPaged: async (page = 1, pageSize = 20): Promise<PagedResult<ProfessionalInterview>> => {
    const response = await trainingApi.get<PagedResult<ProfessionalInterview>>(BASE_PATH, {
      params: { page, size: pageSize },
    });
    return response.data;
  },

  getById: async (id: string): Promise<ProfessionalInterview> => {
    const response = await trainingApi.get<ProfessionalInterview>(`${BASE_PATH}/${id}`);
    return response.data;
  },

  create: async (data: Partial<ProfessionalInterview>): Promise<ProfessionalInterview> => {
    const response = await trainingApi.post<ProfessionalInterview>(BASE_PATH, data);
    return response.data;
  },

  update: async (id: string, data: Partial<ProfessionalInterview>): Promise<ProfessionalInterview> => {
    const response = await trainingApi.put<ProfessionalInterview>(`${BASE_PATH}/${id}`, data);
    return response.data;
  },

  getByEmployeeId: async (employeeId: string): Promise<ProfessionalInterview[]> => {
    const response = await trainingApi.get<ProfessionalInterview[]>(
      `${BASE_PATH}/employee/${employeeId}`,
    );
    return response.data;
  },
};
