import { trainingApi } from '../apiClient';
import { TrainingAction, TrainingSession, PagedResult } from '../../types/training';

const ACTIONS_PATH = '/training-actions';
const SESSIONS_PATH = '/training-sessions';

export const trainingActionService = {
  getById: async (id: string): Promise<TrainingAction> => {
    const response = await trainingApi.get<TrainingAction>(`${ACTIONS_PATH}/${id}`);
    return response.data;
  },

  getAll: async (page = 1, pageSize = 20): Promise<PagedResult<TrainingAction>> => {
    const response = await trainingApi.get<PagedResult<TrainingAction>>(ACTIONS_PATH, {
      params: { page, size: pageSize },
    });
    return response.data;
  },

  getByPlanId: async (planId: string): Promise<TrainingAction[]> => {
    const response = await trainingApi.get<TrainingAction[]>(`${ACTIONS_PATH}/by-plan/${planId}`);
    return response.data;
  },

  create: async (action: Partial<TrainingAction>): Promise<TrainingAction> => {
    const response = await trainingApi.post<TrainingAction>(ACTIONS_PATH, action);
    return response.data;
  },

  update: async (id: string, action: Partial<TrainingAction>): Promise<TrainingAction> => {
    const response = await trainingApi.put<TrainingAction>(`${ACTIONS_PATH}/${id}`, action);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await trainingApi.delete(`${ACTIONS_PATH}/${id}`);
  },
};

export const trainingSessionService = {
  getByActionId: async (actionId: string): Promise<TrainingSession[]> => {
    const response = await trainingApi.get<TrainingSession[]>(`${SESSIONS_PATH}/by-action/${actionId}`);
    return response.data;
  },

  create: async (session: Partial<TrainingSession>): Promise<TrainingSession> => {
    const response = await trainingApi.post<TrainingSession>(SESSIONS_PATH, session);
    return response.data;
  },

  update: async (id: string, session: Partial<TrainingSession>): Promise<TrainingSession> => {
    const response = await trainingApi.put<TrainingSession>(`${SESSIONS_PATH}/${id}`, session);
    return response.data;
  },
};
