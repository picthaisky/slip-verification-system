import { configureStore } from '@reduxjs/toolkit';
import authReducer, { setCredentials, logout } from '../store/slices/authSlice';

describe('Auth Slice', () => {
  let store: any;

  beforeEach(() => {
    store = configureStore({
      reducer: {
        auth: authReducer,
      },
    });
  });

  describe('setCredentials', () => {
    it('should set user and token', () => {
      const user = {
        id: '1',
        email: 'test@example.com',
        name: 'Test User',
        role: 'User' as const,
        createdAt: '2024-01-01',
      };

      const token = 'test-token';

      store.dispatch(setCredentials({ user, token }));

      const state = store.getState().auth;
      expect(state.user).toEqual(user);
      expect(state.token).toEqual(token);
      expect(state.isAuthenticated).toBe(true);
    });
  });

  describe('logout', () => {
    it('should clear user and token', () => {
      // First set credentials
      const user = {
        id: '1',
        email: 'test@example.com',
        name: 'Test User',
        role: 'User' as const,
        createdAt: '2024-01-01',
      };

      store.dispatch(setCredentials({ user, token: 'test-token' }));

      // Then logout
      store.dispatch(logout());

      const state = store.getState().auth;
      expect(state.user).toBeNull();
      expect(state.token).toBeNull();
      expect(state.isAuthenticated).toBe(false);
    });
  });
});
