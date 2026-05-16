import { TestBed } from '@angular/core/testing';
import { Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';

describe('AuthGuard', () => {
  let guard: AuthGuard;
  let authService: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(() => {
    authService = jasmine.createSpyObj('AuthService', ['isLoggedIn', 'isAdmin']);
    router = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        AuthGuard,
        { provide: AuthService, useValue: authService },
        { provide: Router, useValue: router }
      ]
    });

    guard = TestBed.inject(AuthGuard);
  });

  // helper to mock route
  function createRoute(role?: string): ActivatedRouteSnapshot {
    return {
      data: { role }
    } as unknown as ActivatedRouteSnapshot;
  }

  // -----------------------------
  // NOT LOGGED IN
  // -----------------------------
  it('should redirect to login if not logged in', () => {
    authService.isLoggedIn.and.returnValue(false);

    const result = guard.canActivate(createRoute());

    expect(result).toBeFalse();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  // -----------------------------
  // LOGGED IN, NO ROLE REQUIRED
  // -----------------------------
  it('should allow access if logged in and no role required', () => {
    authService.isLoggedIn.and.returnValue(true);

    const result = guard.canActivate(createRoute());

    expect(result).toBeTrue();
  });

  // -----------------------------
  // ADMIN ROUTE - ADMIN USER
  // -----------------------------
  it('should allow admin access for admin user', () => {
    authService.isLoggedIn.and.returnValue(true);
    authService.isAdmin.and.returnValue(true);

    const result = guard.canActivate(createRoute('Admin'));

    expect(result).toBeTrue();
  });

  // -----------------------------
  // ADMIN ROUTE - NON ADMIN USER
  // -----------------------------
  it('should redirect non-admin user trying to access admin route', () => {
    authService.isLoggedIn.and.returnValue(true);
    authService.isAdmin.and.returnValue(false);

    const result = guard.canActivate(createRoute('Admin'));

    expect(result).toBeFalse();
    expect(router.navigate).toHaveBeenCalledWith(['/']);
  });
});