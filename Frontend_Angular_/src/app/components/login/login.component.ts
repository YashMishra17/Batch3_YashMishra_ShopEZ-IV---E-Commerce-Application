import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="auth-page">
      <div class="auth-container">

        <!-- Left Panel -->
        <div class="auth-left">
          <div class="auth-left-content">
            <h1 class="auth-brand">ShopEZ</h1>
            <p class="auth-left-tagline">India's Favourite Online Shopping Destination</p>
            <ul class="auth-features">
              <li><i class="bi bi-check-circle-fill"></i> 1 Crore+ Happy Customers</li>
              <li><i class="bi bi-check-circle-fill"></i> 10,000+ Products</li>
              <li><i class="bi bi-check-circle-fill"></i> Free & Fast Delivery</li>
              <li><i class="bi bi-check-circle-fill"></i> 100% Secure Payments</li>
            </ul>
            <img src="assets/images/laptop.jpg" alt="Shopping" class="auth-hero-img" />
          </div>
        </div>

        <!-- Right Panel: Login Form -->
        <div class="auth-right">
          <div class="auth-form-card">

            <h2 class="auth-form-title">Login</h2>
            <p class="auth-form-subtitle">Get access to your Orders, Wishlist and Recommendations</p>

            <!-- Error -->
            <div class="alert-shopez alert-error mb-3" *ngIf="errorMessage">
              <i class="bi bi-exclamation-circle me-2"></i>{{ errorMessage }}
            </div>

            <!-- Form -->
            <div class="shopez-form-group">
              <label class="shopez-label">Email Address</label>
              <input
                type="email"
                class="shopez-input"
                [(ngModel)]="email"
                placeholder="Enter Email"
                (keyup.enter)="login()"
              />
            </div>

            <div class="shopez-form-group">
              <label class="shopez-label">Password</label>
              <div class="password-wrapper">
                <input
                  [type]="showPassword ? 'text' : 'password'"
                  class="shopez-input"
                  [(ngModel)]="password"
                  placeholder="Enter Password"
                  (keyup.enter)="login()"
                />
                <button class="password-toggle" (click)="showPassword = !showPassword" type="button">
                  <i [class]="showPassword ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
                </button>
              </div>
            </div>

            <p class="terms-text">
              By continuing, you agree to ShopEZ's
              <a href="#">Terms of Use</a> and <a href="#">Privacy Policy</a>.
            </p>

            <button
              class="btn-shopez btn-primary-shopez btn-lg-shopez btn-block"
              (click)="login()"
              [disabled]="loading"
            >
              <span *ngIf="!loading">Login</span>
              <span *ngIf="loading">
                <span class="spinner-border spinner-border-sm me-2"></span>Logging in...
              </span>
            </button>

            <div class="auth-divider">
              <span>OR</span>
            </div>

            <a routerLink="/register" class="btn-shopez btn-outline-shopez btn-lg-shopez btn-block">
              Create New Account
            </a>

            <!-- Demo credentials -->
            <div class="demo-creds">
              <p class="demo-title">Demo Credentials</p>
              <div class="demo-row">
                <span>Admin:</span>
                <code>alice&#64;shopez.com</code>
              </div>

              <div class="demo-row">
                <span>Customer:</span>
                <code>bob&#64;shopez.com</code>
              </div>
            </div>

          </div>
        </div>

      </div>
    </div>
  `,
  styles: [`
    .auth-page {
      min-height: 100vh;
      background: #f1f3f6;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 20px;
    }
    .auth-container {
      display: grid;
      grid-template-columns: 1fr 1fr;
      max-width: 860px;
      width: 100%;
      border-radius: 12px;
      overflow: hidden;
      box-shadow: 0 8px 40px rgba(0,0,0,.15);
    }
    /* Left */
    .auth-left {
      background: linear-gradient(160deg, #2874f0 0%, #0d3b6e 100%);
      padding: 40px 32px;
      display: flex;
      align-items: center;
    }
    .auth-left-content { color: #fff; }
    .auth-brand { font-size: 32px; font-weight: 800; margin-bottom: 8px; }
    .auth-left-tagline { font-size: 16px; opacity: .85; margin-bottom: 24px; line-height: 1.5; }
    .auth-features { list-style: none; padding: 0; margin-bottom: 24px; }
    .auth-features li { display: flex; align-items: center; gap: 10px; font-size: 14px; padding: 6px 0; }
    .auth-features li i { color: #a5d6a7; font-size: 16px; }
    .auth-hero-img { width: 100%; height: 160px; object-fit: cover; border-radius: 8px; opacity: .85; }

    /* Right */
    .auth-right { background: #fff; padding: 40px 32px; }
    .auth-form-title { font-size: 22px; font-weight: 700; color: #212121; margin-bottom: 6px; }
    .auth-form-subtitle { font-size: 13px; color: #878787; margin-bottom: 24px; }

    /* Password wrapper */
    .password-wrapper { position: relative; }
    .password-wrapper .shopez-input { padding-right: 44px; }
    .password-toggle {
      position: absolute; right: 12px; top: 50%;
      transform: translateY(-50%);
      background: none; border: none;
      color: #757575; font-size: 16px; cursor: pointer;
    }

    .terms-text { font-size: 12px; color: #878787; margin-bottom: 16px; line-height: 1.6; }
    .terms-text a { color: #2874f0; }

    /* Divider */
    .auth-divider {
      display: flex; align-items: center; gap: 12px;
      margin: 20px 0; color: #bdbdbd; font-size: 13px;
    }
    .auth-divider::before, .auth-divider::after {
      content: ''; flex: 1; height: 1px; background: #e0e0e0;
    }

    /* Demo creds */
    .demo-creds {
      margin-top: 20px; padding: 14px;
      background: #f8f9fa; border-radius: 8px;
      border: 1px dashed #e0e0e0;
    }
    .demo-title { font-size: 12px; font-weight: 700; color: #424242; margin-bottom: 8px; }
    .demo-row { display: flex; align-items: center; gap: 8px; font-size: 12px; margin-bottom: 4px; }
    .demo-row span { color: #757575; min-width: 70px; }
    .demo-row code { background: #e3f2fd; color: #1565c0; padding: 2px 6px; border-radius: 3px; font-size: 11px; }

    @media(max-width: 640px) {
      .auth-container { grid-template-columns: 1fr; }
      .auth-left { display: none; }
    }
  `]
})
export class LoginComponent {
  email = '';
  password = '';
  loading = false;
  errorMessage = '';
  showPassword = false;

  constructor(private authService: AuthService, private router: Router) {}

  login(): void {
    if (!this.email || !this.password) {
      this.errorMessage = 'Please enter your email and password.';
      return;
    }
    this.loading = true;
    this.errorMessage = '';

    this.authService.login({ Email: this.email, Password: this.password }).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) {
          this.router.navigate(res.data?.Role === 'Admin' ? ['/admin'] : ['/']);
        }
      },
      error: (err) => {
        this.loading = false;
        if (err.status === 401)    this.errorMessage = 'Invalid email or password.';
        else if (err.status === 0) this.errorMessage = 'Cannot connect to server. Is the API running?';
        else                       this.errorMessage = err.error?.message ?? 'Login failed. Please try again.';
      }
    });
  }
}