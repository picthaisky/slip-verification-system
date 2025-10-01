import { authApi } from '../api/endpoints/auth';
import storageService from '../services/storage.service';

// Mock the API client
jest.mock('../api/client', () => ({
  post: jest.fn(),
  get: jest.fn(),
}));

// Mock storage service
jest.mock('../services/storage.service', () => ({
  setAuthToken: jest.fn(),
  setRefreshToken: jest.fn(),
  setObject: jest.fn(),
  getAuthToken: jest.fn(),
  removeAuthToken: jest.fn(),
  removeRefreshToken: jest.fn(),
  removeItem: jest.fn(),
}));

describe('Auth API', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('login', () => {
    it('should store tokens on successful login', async () => {
      const mockResponse = {
        data: {
          token: 'test-token',
          refreshToken: 'test-refresh-token',
          user: {
            id: '1',
            email: 'test@example.com',
            name: 'Test User',
            role: 'User',
            createdAt: '2024-01-01',
          },
        },
      };

      const apiClient = require('../api/client');
      apiClient.post.mockResolvedValue(mockResponse);

      const result = await authApi.login({
        email: 'test@example.com',
        password: 'password',
      });

      expect(storageService.setAuthToken).toHaveBeenCalledWith('test-token');
      expect(storageService.setRefreshToken).toHaveBeenCalledWith('test-refresh-token');
      expect(result).toEqual(mockResponse.data);
    });
  });

  describe('logout', () => {
    it('should clear tokens on logout', async () => {
      await authApi.logout();

      expect(storageService.removeAuthToken).toHaveBeenCalled();
      expect(storageService.removeRefreshToken).toHaveBeenCalled();
    });
  });
});
