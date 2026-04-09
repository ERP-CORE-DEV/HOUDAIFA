import { trainingApi } from '../apiClient';
import { PagedResult } from '../../types/training';

export interface Provider {
  Id: string;
  Name: string;
  Siret: string;
  QualiopiCertified: boolean;
  QualiopiExpirationDate?: string;
  Domaines: string[];
  AverageRating: number;
  IsActive: boolean;
}

const BASE_PATH = '/providers';

export const providerService = {
  getById: async (id: string): Promise<Provider> => {
    const response = await trainingApi.get<Provider>(`${BASE_PATH}/${id}`);
    return response.data;
  },

  getAll: async (page = 1, pageSize = 20): Promise<PagedResult<Provider>> => {
    const response = await trainingApi.get<PagedResult<Provider>>(BASE_PATH, {
      params: { page, size: pageSize },
    });
    return response.data;
  },

  create: async (provider: Partial<Provider>): Promise<Provider> => {
    const response = await trainingApi.post<Provider>(BASE_PATH, provider);
    return response.data;
  },

  update: async (id: string, provider: Partial<Provider>): Promise<Provider> => {
    const response = await trainingApi.put<Provider>(`${BASE_PATH}/${id}`, provider);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await trainingApi.delete(`${BASE_PATH}/${id}`);
  },
};
