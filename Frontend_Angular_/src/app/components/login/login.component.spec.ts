/// <reference types="jasmine" />

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LoginComponent } from './login.component';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { RouterTestingModule } from '@angular/router/testing';
import { FormsModule } from '@angular/forms';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let router: Router;

  beforeEach(async () => {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['login']);

    await TestBed.configureTestingModule({
      imports: [
        LoginComponent,
        RouterTestingModule, 
        FormsModule
      ],
      providers: [
        { provide: AuthService, useValue: authServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);

    spyOn(router, 'navigate');

    fixture.detectChanges();
  });

  // ----------- TESTS -----------

  it('should show error if email or password is empty', () => {
    component.email = '';
    component.password = '';

    component.login();

    expect(component.errorMessage).toBe('Please enter your email and password.');
  });

  it('should login and navigate to admin dashboard', () => {
    component.email = 'admin@test.com';
    component.password = '123';

    authServiceSpy.login.and.returnValue(of({
      success: true,
      data: {
        UserId: 1,
        Name: 'Admin',
        Email: 'admin@test.com',
        Token: 'abc',
        ExpiresAt: '',
        Role: 'Admin'
      }
    }));

    component.login();

    expect(router.navigate).toHaveBeenCalledWith(['/admin']);
  });

  it('should login and navigate to home for customer', () => {
    component.email = 'user@test.com';
    component.password = '123';

    authServiceSpy.login.and.returnValue(of({
      success: true,
      data: {
        UserId: 2,
        Name: 'User',
        Email: 'user@test.com',
        Token: 'xyz',
        ExpiresAt: '',
        Role: 'Customer'
      }
    }));

    component.login();

    expect(router.navigate).toHaveBeenCalledWith(['/']);
  });

  it('should show invalid credentials error for 401', () => {
    authServiceSpy.login.and.returnValue(
      throwError(() => ({ status: 401 }))
    );

    component.email = 'x';
    component.password = 'y';

    component.login();

    expect(component.errorMessage).toBe('Invalid email or password.');
  });

  it('should show server connection error when status is 0', () => {
    authServiceSpy.login.and.returnValue(
      throwError(() => ({ status: 0 }))
    );

    component.email = 'x';
    component.password = 'y';

    component.login();

    expect(component.errorMessage).toBe('Cannot connect to server. Is the API running?');
  });

  it('should show backend error message if provided', () => {
    authServiceSpy.login.and.returnValue(
      throwError(() => ({
        status: 500,
        error: { message: 'Custom backend error' }
      }))
    );

    component.email = 'x';
    component.password = 'y';

    component.login();

    expect(component.errorMessage).toBe('Custom backend error');
  });

  it('should show default error message if backend message missing', () => {
    authServiceSpy.login.and.returnValue(
      throwError(() => ({
        status: 500,
        error: {}
      }))
    );

    component.email = 'x';
    component.password = 'y';

    component.login();

    expect(component.errorMessage).toBe('Login failed. Please try again.');
  });

});