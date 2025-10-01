import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AuthService, LoginRequest, LoginResponse, User } from './auth.service';
import { ApiService } from './api.service';

describe('AuthService', () => {
  let service: AuthService;
  let apiServiceSpy: jasmine.SpyObj<ApiService>;
  let routerSpy: jasmine.SpyObj<Router>;

  const mockUser: User = {
    id: '1',
    username: 'testuser',
    email: 'test@example.com',
    role: 'User',
    firstName: 'Test',
    lastName: 'User'
  };

  const mockLoginResponse: LoginResponse = {
    token: 'mock-jwt-token',
    refreshToken: 'mock-refresh-token',
    user: mockUser
  };

  beforeEach(() => {
    const apiSpy = jasmine.createSpyObj('ApiService', ['post', 'get']);
    const routerSpyObj = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        { provide: ApiService, useValue: apiSpy },
        { provide: Router, useValue: routerSpyObj }
      ]
    });

    service = TestBed.inject(AuthService);
    apiServiceSpy = TestBed.inject(ApiService) as jasmine.SpyObj<ApiService>;
    routerSpy = TestBed.inject(Router) as jasmine.SpyObj<Router>;

    // Clear localStorage before each test
    localStorage.clear();
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('login', () => {
    it('should login successfully and store auth data', (done) => {
      const credentials: LoginRequest = {
        email: 'test@example.com',
        password: 'password123'
      };

      apiServiceSpy.post.and.returnValue(of(mockLoginResponse));

      service.login(credentials).subscribe(response => {
        expect(response).toEqual(mockLoginResponse);
        expect(service.isAuthenticated()).toBe(true);
        expect(service.currentUser()).toEqual(mockUser);
        expect(localStorage.getItem('auth_token')).toBe('mock-jwt-token');
        expect(localStorage.getItem('refresh_token')).toBe('mock-refresh-token');
        done();
      });
    });

    it('should handle login error', (done) => {
      const credentials: LoginRequest = {
        email: 'test@example.com',
        password: 'wrong-password'
      };

      apiServiceSpy.post.and.returnValue(throwError(() => new Error('Invalid credentials')));

      service.login(credentials).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.message).toBe('Invalid credentials');
          expect(service.isAuthenticated()).toBe(false);
          done();
        }
      });
    });
  });

  describe('logout', () => {
    it('should clear auth data and redirect to login', () => {
      // Set up authenticated state
      localStorage.setItem('auth_token', 'mock-token');
      localStorage.setItem('refresh_token', 'mock-refresh');
      localStorage.setItem('current_user', JSON.stringify(mockUser));
      service.currentUser.set(mockUser);
      service.isAuthenticated.set(true);

      apiServiceSpy.post.and.returnValue(of({}));

      service.logout();

      expect(service.isAuthenticated()).toBe(false);
      expect(service.currentUser()).toBeNull();
      expect(localStorage.getItem('auth_token')).toBeNull();
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/auth/login']);
    });
  });

  describe('getToken', () => {
    it('should return token from localStorage', () => {
      const token = 'test-token';
      localStorage.setItem('auth_token', token);

      expect(service.getToken()).toBe(token);
    });

    it('should return null when no token exists', () => {
      expect(service.getToken()).toBeNull();
    });
  });

  describe('isUserInRole', () => {
    it('should return true for matching role', () => {
      service.currentUser.set(mockUser);

      expect(service.isUserInRole('User')).toBe(true);
    });

    it('should return false for non-matching role', () => {
      service.currentUser.set(mockUser);

      expect(service.isUserInRole('Admin')).toBe(false);
    });

    it('should return false when no user is logged in', () => {
      service.currentUser.set(null);

      expect(service.isUserInRole('User')).toBe(false);
    });
  });

  describe('loadUserFromStorage', () => {
    it('should load user data from localStorage on init', () => {
      localStorage.setItem('auth_token', 'mock-token');
      localStorage.setItem('current_user', JSON.stringify(mockUser));

      // Create new service instance to trigger constructor
      const newService = new AuthService();

      expect(newService.currentUser()).toEqual(mockUser);
      expect(newService.isAuthenticated()).toBe(true);
    });

    it('should handle corrupted user data in localStorage', () => {
      localStorage.setItem('auth_token', 'mock-token');
      localStorage.setItem('current_user', 'invalid-json');

      const newService = new AuthService();

      expect(newService.currentUser()).toBeNull();
      expect(newService.isAuthenticated()).toBe(false);
    });
  });
});
