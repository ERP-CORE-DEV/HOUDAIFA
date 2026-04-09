import { trainingApi } from '../apiClient';
import { PagedResult } from '../../types/training';

export type AlternanceContractType = 'Apprentissage' | 'Professionnalisation';
export type AlternanceStatus = 'EnCours' | 'Termine' | 'Rompu' | 'Suspendu';

export interface AlternanceContract {
  Id: string;
  ApprentiId: string;
  ApprentiNom: string;
  ApprentiAge: number;
  MaitreApprentissageId: string;
  MaitreApprentissageNom: string;
  ContractType: AlternanceContractType;
  StartDate: string;
  EndDate: string;
  Remuneration: number;
  Status: AlternanceStatus;
  CfaId?: string;
  CfaNom?: string;
  IsActive: boolean;
}

const BASE_PATH = '/alternance-contracts';

export const alternanceService = {
  getById: async (id: string): Promise<AlternanceContract> => {
    const response = await trainingApi.get<AlternanceContract>(`${BASE_PATH}/${id}`);
    return response.data;
  },

  getAll: async (page = 1, pageSize = 20): Promise<PagedResult<AlternanceContract>> => {
    const response = await trainingApi.get<PagedResult<AlternanceContract>>(BASE_PATH, {
      params: { page, size: pageSize },
    });
    return response.data;
  },

  create: async (contract: Partial<AlternanceContract>): Promise<AlternanceContract> => {
    const response = await trainingApi.post<AlternanceContract>(BASE_PATH, contract);
    return response.data;
  },

  update: async (id: string, contract: Partial<AlternanceContract>): Promise<AlternanceContract> => {
    const response = await trainingApi.put<AlternanceContract>(`${BASE_PATH}/${id}`, contract);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await trainingApi.delete(`${BASE_PATH}/${id}`);
  },
};
