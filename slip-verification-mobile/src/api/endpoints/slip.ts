import apiClient from '../client';
import { Slip } from '../../types';

export const slipApi = {
  uploadSlip: async (
    orderId: string, 
    file: any, 
    onUploadProgress?: (progress: number) => void
  ): Promise<Slip> => {
    const formData = new FormData();
    formData.append('orderId', orderId);
    formData.append('file', {
      uri: file.uri,
      type: file.type || 'image/jpeg',
      name: file.fileName || 'slip.jpg',
    } as any);

    const response = await apiClient.uploadFile<Slip>(
      '/slips/verify',
      formData,
      (progressEvent) => {
        if (onUploadProgress && progressEvent.total) {
          const progress = Math.round((progressEvent.loaded * 100) / progressEvent.total);
          onUploadProgress(progress);
        }
      }
    );

    return response.data;
  },

  getSlipById: async (id: string): Promise<Slip> => {
    const response = await apiClient.get<Slip>(`/slips/${id}`);
    return response.data;
  },

  getSlips: async (params?: {
    page?: number;
    pageSize?: number;
    status?: string;
  }): Promise<{ items: Slip[]; total: number; page: number; pageSize: number }> => {
    const response = await apiClient.get<{ items: Slip[]; total: number; page: number; pageSize: number }>(
      '/slips',
      params
    );
    return response.data;
  },

  getSlipsByOrder: async (orderId: string): Promise<Slip[]> => {
    const response = await apiClient.get<Slip[]>(`/orders/${orderId}/slips`);
    return response.data;
  },
};
