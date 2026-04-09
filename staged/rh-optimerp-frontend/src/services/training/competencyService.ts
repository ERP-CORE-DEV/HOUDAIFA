import { trainingApi } from '../apiClient';
import { Competency, EmployeeCompetencyAssessment, PagedResult } from '../../types/training';

const COMPETENCIES_PATH = '/competencies';
const ASSESSMENTS_PATH = '/competency-assessments';

export const competencyService = {
  getPaged: async (page = 1, pageSize = 20): Promise<PagedResult<Competency>> => {
    const response = await trainingApi.get<PagedResult<Competency>>(COMPETENCIES_PATH, {
      params: { page, size: pageSize },
    });
    return response.data;
  },

  getById: async (id: string): Promise<Competency> => {
    const response = await trainingApi.get<Competency>(`${COMPETENCIES_PATH}/${id}`);
    return response.data;
  },

  create: async (data: Partial<Competency>): Promise<Competency> => {
    const response = await trainingApi.post<Competency>(COMPETENCIES_PATH, data);
    return response.data;
  },

  update: async (id: string, data: Partial<Competency>): Promise<Competency> => {
    const response = await trainingApi.put<Competency>(`${COMPETENCIES_PATH}/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await trainingApi.delete(`${COMPETENCIES_PATH}/${id}`);
  },

  getAssessments: async (employeeId: string): Promise<EmployeeCompetencyAssessment[]> => {
    const response = await trainingApi.get<EmployeeCompetencyAssessment[]>(
      `${ASSESSMENTS_PATH}/employee/${employeeId}`,
    );
    return response.data;
  },
};
