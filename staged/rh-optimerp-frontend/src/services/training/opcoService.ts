import { trainingApi } from '../apiClient';
import { PagedResult, FundingStatus } from '../../types/training';

export interface Opco {
  Id: string;
  Name: string;
  Code: string;
  ConventionCollective: string;
  IsActive: boolean;
}

export interface FundingRequest {
  Id: string;
  OpcoId: string;
  OpcoName: string;
  EmployeeId: string;
  TrainingActionId: string;
  Amount: number;
  Status: FundingStatus;
  RequestDate: string;
  DecisionDate?: string;
  Comments?: string;
}

const OPCO_PATH = '/opcos';
const FUNDING_PATH = '/funding-requests';

export const opcoService = {
  getAll: async (page = 1, pageSize = 20): Promise<PagedResult<Opco>> => {
    const response = await trainingApi.get<PagedResult<Opco>>(OPCO_PATH, {
      params: { page, size: pageSize },
    });
    return response.data;
  },

  getById: async (id: string): Promise<Opco> => {
    const response = await trainingApi.get<Opco>(`${OPCO_PATH}/${id}`);
    return response.data;
  },

  create: async (opco: Partial<Opco>): Promise<Opco> => {
    const response = await trainingApi.post<Opco>(OPCO_PATH, opco);
    return response.data;
  },

  update: async (id: string, opco: Partial<Opco>): Promise<Opco> => {
    const response = await trainingApi.put<Opco>(`${OPCO_PATH}/${id}`, opco);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await trainingApi.delete(`${OPCO_PATH}/${id}`);
  },

  getAllFundingRequests: async (page = 1, pageSize = 20): Promise<PagedResult<FundingRequest>> => {
    const response = await trainingApi.get<PagedResult<FundingRequest>>(FUNDING_PATH, {
      params: { page, size: pageSize },
    });
    return response.data;
  },

  createFundingRequest: async (request: Partial<FundingRequest>): Promise<FundingRequest> => {
    const response = await trainingApi.post<FundingRequest>(FUNDING_PATH, request);
    return response.data;
  },

  approveFundingRequest: async (id: string): Promise<FundingRequest> => {
    const response = await trainingApi.post<FundingRequest>(`${FUNDING_PATH}/${id}/approve`);
    return response.data;
  },

  rejectFundingRequest: async (id: string): Promise<FundingRequest> => {
    const response = await trainingApi.post<FundingRequest>(`${FUNDING_PATH}/${id}/reject`);
    return response.data;
  },
};
