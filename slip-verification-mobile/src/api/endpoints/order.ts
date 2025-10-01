import apiClient from '../client';
import { Order } from '../../types';

export const orderApi = {
  getOrders: async (params?: {
    page?: number;
    pageSize?: number;
    status?: string;
  }): Promise<{ items: Order[]; total: number; page: number; pageSize: number }> => {
    const response = await apiClient.get<{ items: Order[]; total: number; page: number; pageSize: number }>(
      '/orders',
      params
    );
    return response.data;
  },

  getOrderById: async (id: string): Promise<Order> => {
    const response = await apiClient.get<Order>(`/orders/${id}`);
    return response.data;
  },

  createOrder: async (data: {
    amount: number;
    customerId: string;
    customerName: string;
    description?: string;
  }): Promise<Order> => {
    const response = await apiClient.post<Order>('/orders', data);
    return response.data;
  },
};
