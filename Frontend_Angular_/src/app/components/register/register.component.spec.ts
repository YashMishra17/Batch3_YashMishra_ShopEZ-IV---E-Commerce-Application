/// <reference types="jasmine" />

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RegisterComponent } from './register.component';
import { Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { AuthService } from '../../services/auth.service';
import { of, throwError } from 'rxjs';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let router: Router;

  beforeEach(async () => {
    // Create a fake AuthService (we control responses manually)
    authServiceSpy = jasmine.createSpyObj('AuthService', ['register']);

    await TestBed.configureTestingModule({
      imports: [
        RegisterComponent,
        RouterTestingModule // Required because template uses routerLink
      ],
      providers: [
        { provide: AuthService, useValue: authServiceSpy } // inject mock instead of real service
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;

    // Get router instance to spy navigation
    router = TestBed.inject(Router);

    fixture.detectChanges();
  });

  // Basic sanity check
  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // No input → should fail validation
  it('should show error if fields are empty', () => {
    component.register();
    expect(component.errorMessage).toBe('All fields are required.');
  });

  // Password too short
  it('should validate password length', () => {
    component.name = 'Test';
    component.email = 'test@test.com';
    component.password = '123';
    component.confirmPassword = '123';

    component.register();

    expect(component.errorMessage).toBe('Password must be at least 6 characters.');
  });

  // Password mismatch
  it('should validate password mismatch', () => {
    component.name = 'Test';
    component.email = 'test@test.com';
    component.password = '123456';
    component.confirmPassword = '654321';

    component.register();

    expect(component.errorMessage).toBe('Passwords do not match.');
  });

  // Successful API call → should navigate
  it('should call register API and navigate on success', () => {
    spyOn(router, 'navigate'); // track navigation calls

    // simulate successful backend response
    authServiceSpy.register.and.returnValue(of({ success: true }));

    component.name = 'Test';
    component.email = 'test@test.com';
    component.password = '123456';
    component.confirmPassword = '123456';

    component.register();

    expect(authServiceSpy.register).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/']);
  });

  // 409 → duplicate email
  it('should handle 409 error (duplicate email)', () => {
    authServiceSpy.register.and.returnValue(
      throwError(() => ({ status: 409 }))
    );

    component.name = 'Test';
    component.email = 'test@test.com';
    component.password = '123456';
    component.confirmPassword = '123456';

    component.register();

    expect(component.errorMessage).toBe(
      'An account with this email already exists.'
    );
  });

  // API not reachable
  it('should handle server not reachable error', () => {
    authServiceSpy.register.and.returnValue(
      throwError(() => ({ status: 0 }))
    );

    component.name = 'Test';
    component.email = 'test@test.com';
    component.password = '123456';
    component.confirmPassword = '123456';

    component.register();

    expect(component.errorMessage).toBe(
      'Cannot connect to server. Is the API running?'
    );
  });

  // Any other backend error
  it('should handle generic error', () => {
    authServiceSpy.register.and.returnValue(
      throwError(() => ({
        status: 500,
        error: { message: 'Something failed' }
      }))
    );

    component.name = 'Test';
    component.email = 'test@test.com';
    component.password = '123456';
    component.confirmPassword = '123456';

    component.register();

    expect(component.errorMessage).toBe('Something failed');
  });

  // Password strength logic (pure computed properties — no API involved)

  it('should calculate weak password strength', () => {
    component.password = '123';
    expect(component.pwdStrengthLabel).toBe('Weak');
    expect(component.pwdStrengthWidth).toBe('33%');
  });

  it('should calculate moderate password strength', () => {
    component.password = '1234567';
    expect(component.pwdStrengthLabel).toBe('Moderate');
    expect(component.pwdStrengthWidth).toBe('66%');
  });

  it('should calculate strong password strength', () => {
    component.password = '12345678901';
    expect(component.pwdStrengthLabel).toBe('Strong');
    expect(component.pwdStrengthWidth).toBe('100%');
  });
});
