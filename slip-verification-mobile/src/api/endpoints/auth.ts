import apiClient from '../client';
import { LoginRequest, LoginResponse, RegisterRequest, User } from '../../types';
import storageService, { STORAGE_KEYS } from '../../services/storage.service';

export const authApi = {
  login: async (credentials: LoginRequest): Promise<LoginResponse> => {
    const response = await apiClient.post<LoginResponse>('/auth/login', credentials);
    
    // Store tokens
    await storageService.setAuthToken(response.data.token);
    await storageService.setRefreshToken(response.data.refreshToken);
    await storageService.setObject(STORAGE_KEYS.USER_DATA, response.data.user);
    
    return response.data;
  },

  register: async (data: RegisterRequest): Promise<User> => {
    const response = await apiClient.post<User>('/auth/register', data);
    return response.data;
  },

  logout: async (): Promise<void> => {
    await storageService.removeAuthToken();
    await storageService.removeRefreshToken();
    await storageService.removeItem(STORAGE_KEYS.USER_DATA);
  },

  getCurrentUser: async (): Promise<User> => {
    const response = await apiClient.get<User>('/auth/me');
    return response.data;
  },

  refreshToken: async (refreshToken: string): Promise<LoginResponse> => {
    const response = await apiClient.post<LoginResponse>('/auth/refresh', { refreshToken });
    
    // Update tokens
    await storageService.setAuthToken(response.data.token);
    await storageService.setRefreshToken(response.data.refreshToken);
    
    return response.data;
  },
};
