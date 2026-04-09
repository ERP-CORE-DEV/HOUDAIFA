import { trainingApi } from '../apiClient';
import { PagedResult } from '../../types/training';

export interface ElearningCourse {
  Id: string;
  Title: string;
  CourseType: string;
  Platform: string;
  DurationHours: number;
  CompletionPercentage: number;
  IsActive: boolean;
}

const BASE_PATH = '/elearning';

export const elearningService = {
  getById: async (id: string): Promise<ElearningCourse> => {
    const response = await trainingApi.get<ElearningCourse>(`${BASE_PATH}/${id}`);
    return response.data;
  },

  getAll: async (page = 1, pageSize = 20): Promise<PagedResult<ElearningCourse>> => {
    const response = await trainingApi.get<PagedResult<ElearningCourse>>(BASE_PATH, {
      params: { page, size: pageSize },
    });
    return response.data;
  },

  create: async (course: Partial<ElearningCourse>): Promise<ElearningCourse> => {
    const response = await trainingApi.post<ElearningCourse>(BASE_PATH, course);
    return response.data;
  },

  update: async (id: string, course: Partial<ElearningCourse>): Promise<ElearningCourse> => {
    const response = await trainingApi.put<ElearningCourse>(`${BASE_PATH}/${id}`, course);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await trainingApi.delete(`${BASE_PATH}/${id}`);
  },
};
