/// <reference types="jasmine" />

import { TestBed } from '@angular/core/testing';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { HTTP_INTERCEPTORS, HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

import { AuthInterceptor } from './auth.interceptor';

describe('AuthInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let router: jasmine.SpyObj<Router>;

  beforeEach(() => {
    router = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        { provide: Router, useValue: router },

        //  Pull interceptors from DI (THIS is key)
        provideHttpClient(withInterceptorsFromDi()),

        provideHttpClientTesting(),

        //  Register your class-based interceptor
        {
          provide: HTTP_INTERCEPTORS,
          useClass: AuthInterceptor,
          multi: true
        }
      ]
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);

    localStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should attach token', () => {
    localStorage.setItem('shopez_token', '123');

    http.get('/test').subscribe();

    const req = httpMock.expectOne('/test');
    expect(req.request.headers.get('Authorization')).toBe('Bearer 123');

    req.flush({});
  });
});