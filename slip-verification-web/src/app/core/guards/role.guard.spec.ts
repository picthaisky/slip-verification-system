import { TestBed } from '@angular/core/testing';
import { Router, ActivatedRouteSnapshot } from '@angular/router';
import { roleGuard } from './role.guard';
import { AuthService } from '../services/auth.service';
import { signal } from '@angular/core';

describe('roleGuard', () => {
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let routerSpy: jasmine.SpyObj<Router>;
  let mockRoute: ActivatedRouteSnapshot;

  beforeEach(() => {
    const authSpy = jasmine.createSpyObj('AuthService', ['hasAnyRole'], {
      isAuthenticated: signal(false)
    });
    const routerSpyObj = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authSpy },
        { provide: Router, useValue: routerSpyObj }
      ]
    });

    authServiceSpy = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    routerSpy = TestBed.inject(Router) as jasmine.SpyObj<Router>;

    // Create mock route
    mockRoute = {
      data: { roles: ['Admin'] }
    } as any;
  });

  it('should allow access when user has required role', () => {
    authServiceSpy.isAuthenticated = signal(true);
    authServiceSpy.hasAnyRole.and.returnValue(true);

    const result = TestBed.runInInjectionContext(() => roleGuard(mockRoute, {} as any));

    expect(result).toBe(true);
    expect(authServiceSpy.hasAnyRole).toHaveBeenCalledWith(['Admin']);
    expect(routerSpy.navigate).not.toHaveBeenCalled();
  });

  it('should deny access when user does not have required role', () => {
    authServiceSpy.isAuthenticated = signal(true);
    authServiceSpy.hasAnyRole.and.returnValue(false);

    const result = TestBed.runInInjectionContext(() => roleGuard(mockRoute, {} as any));

    expect(result).toBe(false);
    expect(authServiceSpy.hasAnyRole).toHaveBeenCalledWith(['Admin']);
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/unauthorized']);
  });

  it('should redirect to login when user is not authenticated', () => {
    authServiceSpy.isAuthenticated = signal(false);

    const result = TestBed.runInInjectionContext(() => roleGuard(mockRoute, {} as any));

    expect(result).toBe(false);
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should allow access when no roles are specified', () => {
    authServiceSpy.isAuthenticated = signal(true);
    mockRoute.data = {}; // No roles specified

    const result = TestBed.runInInjectionContext(() => roleGuard(mockRoute, {} as any));

    expect(result).toBe(true);
    expect(authServiceSpy.hasAnyRole).not.toHaveBeenCalled();
  });

  it('should check multiple roles', () => {
    authServiceSpy.isAuthenticated = signal(true);
    authServiceSpy.hasAnyRole.and.returnValue(true);
    mockRoute.data = { roles: ['Admin', 'User'] };

    const result = TestBed.runInInjectionContext(() => roleGuard(mockRoute, {} as any));

    expect(result).toBe(true);
    expect(authServiceSpy.hasAnyRole).toHaveBeenCalledWith(['Admin', 'User']);
  });
});
