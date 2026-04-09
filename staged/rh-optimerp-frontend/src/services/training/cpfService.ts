import { trainingApi } from '../apiClient';
import { CpfAccount } from '../../types/training';

const CPF_PATH = '/cpf';

export const cpfService = {
  getByEmployeeId: async (employeeId: string): Promise<CpfAccount> => {
    const response = await trainingApi.get<CpfAccount>(`${CPF_PATH}/employee/${employeeId}`);
    return response.data;
  },

  getById: async (id: string): Promise<CpfAccount> => {
    const response = await trainingApi.get<CpfAccount>(`${CPF_PATH}/${id}`);
    return response.data;
  },

  applyAnnualCredit: async (id: string): Promise<CpfAccount> => {
    const response = await trainingApi.post<CpfAccount>(`${CPF_PATH}/${id}/annual-credit`);
    return response.data;
  },
};
