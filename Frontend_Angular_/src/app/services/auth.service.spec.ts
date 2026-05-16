import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';
import { AuthResponse } from '../models/auth.model';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  const mockUser: AuthResponse = {
    UserId: 1,
    Name: 'John Doe',
    Email: 'john@test.com',
    Role: 'Admin',
    Token: 'fake-token',
    ExpiresAt: new Date().toISOString()
  };

  // Helper to create fresh service instance
  function createService(): AuthService {
    TestBed.resetTestingModule(); // FORCE new instance

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    return TestBed.inject(AuthService);
  }

  beforeEach(() => {
    localStorage.clear();

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  // -----------------------------
  // LOGIN
  // -----------------------------
  it('should login and store token + user', () => {
    service.login({ Email: 'john@test.com', Password: '123456' })
      .subscribe(res => {
        expect(res.success).toBeTrue();
      });

    const req = httpMock.expectOne(`${environment.apiUrl}/auth/login`);
    req.flush({ success: true, data: mockUser });

    expect(localStorage.getItem('shopez_token')).toBe('fake-token');
    expect(JSON.parse(localStorage.getItem('shopez_user')!)).toEqual(mockUser);
  });

  // -----------------------------
  // REGISTER
  // -----------------------------
  it('should register and store user', () => {
    service.register({
      Name: 'John',
      Email: 'john@test.com',
      Password: '123456',
      Role: 'Customer'
    }).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/auth/register`);
    req.flush({ success: true, data: mockUser });

    expect(localStorage.getItem('shopez_token')).toBeTruthy();
  });

  // -----------------------------
  // LOGOUT
  // -----------------------------
  it('should clear storage on logout', () => {
    localStorage.setItem('shopez_token', '123');
    localStorage.setItem('shopez_user', JSON.stringify(mockUser));

    service.logout();

    expect(localStorage.getItem('shopez_token')).toBeNull();
    expect(localStorage.getItem('shopez_user')).toBeNull();
  });

  // -----------------------------
  // IS LOGGED IN
  // -----------------------------
  it('should return true if token exists', () => {
    localStorage.setItem('shopez_token', 'abc');

    const freshService = createService();

    expect(freshService.isLoggedIn()).toBeTrue();
  });

  it('should return false if token does not exist', () => {
    const freshService = createService();
    expect(freshService.isLoggedIn()).toBeFalse();
  });

  // -----------------------------
  // IS ADMIN (FIXED PROPERLY)
  // -----------------------------
  it('should return true for admin user', () => {
    localStorage.setItem('shopez_user', JSON.stringify(mockUser));

    const freshService = createService();

    expect(freshService.isAdmin()).toBeTrue();
  });

  // -----------------------------
  // GET CURRENT USER
  // -----------------------------
  it('should return current user', () => {
    localStorage.setItem('shopez_user', JSON.stringify(mockUser));

    const freshService = createService();

    expect(freshService.getCurrentUser()).toEqual(mockUser);
  });

  // -----------------------------
  // GET USER ID
  // -----------------------------
  it('should return correct user id', () => {
    localStorage.setItem('shopez_user', JSON.stringify(mockUser));

    const freshService = createService();

    expect(freshService.getUserId()).toBe(1);
  });

  it('should return 0 if no user exists', () => {
    localStorage.removeItem('shopez_user');

    const freshService = createService();

    expect(freshService.getUserId()).toBe(0);
  });
});