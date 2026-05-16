import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { LoginRequest, RegisterRequest, AuthResponse } from '../models/auth.model';
import { ApiResponse } from '../models/api-response.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = environment.apiUrl;
  private currentUserSubject = new BehaviorSubject<AuthResponse | null>(this.getUserFromStorage());
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {}

  login(credentials: LoginRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/auth/login`, credentials).pipe(
      tap(response => {
        if (response.success && response.data) {
          localStorage.setItem('shopez_token', response.data.Token);
          localStorage.setItem('shopez_user', JSON.stringify(response.data));
          this.currentUserSubject.next(response.data);
        }
      })
    );
  }

  register(data: RegisterRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/auth/register`, data).pipe(
      tap(response => {
        if (response.success && response.data) {
          localStorage.setItem('shopez_token', response.data.Token);
          localStorage.setItem('shopez_user', JSON.stringify(response.data));
          this.currentUserSubject.next(response.data);
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem('shopez_token');
    localStorage.removeItem('shopez_user');
    this.currentUserSubject.next(null);
  }

  isLoggedIn(): boolean { return !!localStorage.getItem('shopez_token'); }
  isAdmin(): boolean { return this.currentUserSubject.value?.Role === 'Admin'; }
  getCurrentUser(): AuthResponse | null { return this.currentUserSubject.value; }
  getUserId(): number { return this.currentUserSubject.value?.UserId ?? 0; }

  private getUserFromStorage(): AuthResponse | null {
    const stored = localStorage.getItem('shopez_user');
    return stored ? JSON.parse(stored) : null;
  }
}