import apiClient from '../client';
import { Notification } from '../../types';

export const notificationApi = {
  getNotifications: async (params?: {
    page?: number;
    pageSize?: number;
    isRead?: boolean;
  }): Promise<{ items: Notification[]; total: number; page: number; pageSize: number }> => {
    const response = await apiClient.get<{ items: Notification[]; total: number; page: number; pageSize: number }>(
      '/notifications',
      params
    );
    return response.data;
  },

  markAsRead: async (id: string): Promise<void> => {
    await apiClient.patch(`/notifications/${id}/read`, {});
  },

  markAllAsRead: async (): Promise<void> => {
    await apiClient.post('/notifications/mark-all-read', {});
  },

  deleteNotification: async (id: string): Promise<void> => {
    await apiClient.delete(`/notifications/${id}`);
  },
};
