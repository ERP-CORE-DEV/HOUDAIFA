import { trainingApi } from '../apiClient';
import { CertificationRncp, VaeProject, PagedResult } from '../../types/training';

const CERTIFICATIONS_PATH = '/certifications';
const VAE_PATH = '/vae-projects';

export const certificationService = {
  getPaged: async (page = 1, pageSize = 20): Promise<PagedResult<CertificationRncp>> => {
    const response = await trainingApi.get<PagedResult<CertificationRncp>>(CERTIFICATIONS_PATH, {
      params: { page, size: pageSize },
    });
    return response.data;
  },

  getById: async (id: string): Promise<CertificationRncp> => {
    const response = await trainingApi.get<CertificationRncp>(`${CERTIFICATIONS_PATH}/${id}`);
    return response.data;
  },

  create: async (data: Partial<CertificationRncp>): Promise<CertificationRncp> => {
    const response = await trainingApi.post<CertificationRncp>(CERTIFICATIONS_PATH, data);
    return response.data;
  },

  update: async (id: string, data: Partial<CertificationRncp>): Promise<CertificationRncp> => {
    const response = await trainingApi.put<CertificationRncp>(`${CERTIFICATIONS_PATH}/${id}`, data);
    return response.data;
  },

  getVaeProjects: async (page = 1, pageSize = 20): Promise<PagedResult<VaeProject>> => {
    const response = await trainingApi.get<PagedResult<VaeProject>>(VAE_PATH, {
      params: { page, size: pageSize },
    });
    return response.data;
  },

  createVae: async (data: Partial<VaeProject>): Promise<VaeProject> => {
    const response = await trainingApi.post<VaeProject>(VAE_PATH, data);
    return response.data;
  },
};
