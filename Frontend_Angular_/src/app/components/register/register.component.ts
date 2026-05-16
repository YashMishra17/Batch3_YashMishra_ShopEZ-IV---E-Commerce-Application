import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="auth-page">
      <div class="auth-container">

        <!-- Left Panel -->
        <div class="auth-left">
          <div class="auth-left-content">
            <h1 class="auth-brand">ShopEZ</h1>
            <p class="auth-left-tagline">Join millions of happy shoppers!</p>
            <ul class="auth-features">
              <li><i class="bi bi-check-circle-fill"></i> Exclusive Member Discounts</li>
              <li><i class="bi bi-check-circle-fill"></i> Track Your Orders Easily</li>
              <li><i class="bi bi-check-circle-fill"></i> Priority Customer Support</li>
              <li><i class="bi bi-check-circle-fill"></i> Faster Checkout</li>
            </ul>
            <img src="assets/images/mobile.jpg" alt="Register" class="auth-hero-img" />
          </div>
        </div>

        <!-- Right Panel -->
        <div class="auth-right">
          <div class="auth-form-card">

            <h2 class="auth-form-title">Create Account</h2>
            <p class="auth-form-subtitle">Sign up and start shopping today</p>

            <div class="alert-shopez alert-error mb-3" *ngIf="errorMessage">
              <i class="bi bi-exclamation-circle me-2"></i>{{ errorMessage }}
            </div>

            <div class="shopez-form-group">
              <label class="shopez-label">Full Name</label>
              <input type="text" class="shopez-input" [(ngModel)]="name" placeholder="Enter your full name" />
            </div>

            <div class="shopez-form-group">
              <label class="shopez-label">Email Address</label>
              <input type="email" class="shopez-input" [(ngModel)]="email" placeholder="Enter Email" />
            </div>

            <div class="shopez-form-group">
              <label class="shopez-label">Password</label>
              <div class="password-wrapper">
                <input
                  [type]="showPassword ? 'text' : 'password'"
                  class="shopez-input"
                  [(ngModel)]="password"
                  placeholder="Min 6 characters"
                />
                <button class="password-toggle" (click)="showPassword = !showPassword" type="button">
                  <i [class]="showPassword ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
                </button>
              </div>
              <!-- Password strength -->
              <div class="pwd-strength mt-1" *ngIf="password.length > 0">
                <div class="pwd-bar" [style.width]="pwdStrengthWidth" [style.background]="pwdStrengthColor"></div>
                <span class="pwd-label" [style.color]="pwdStrengthColor">{{ pwdStrengthLabel }}</span>
              </div>
            </div>

            <div class="shopez-form-group">
              <label class="shopez-label">Confirm Password</label>
              <input type="password" class="shopez-input" [(ngModel)]="confirmPassword" placeholder="Re-enter Password" />
              <span class="text-danger small" *ngIf="confirmPassword && password !== confirmPassword">
                Passwords do not match
              </span>
            </div>

            <p class="terms-text">
              By creating an account, you agree to ShopEZ's
              <a href="#">Terms of Use</a> and <a href="#">Privacy Policy</a>.
            </p>

            <button
              class="btn-shopez btn-primary-shopez btn-lg-shopez btn-block"
              (click)="register()"
              [disabled]="loading"
            >
              <span *ngIf="!loading">Create Account</span>
              <span *ngIf="loading">
                <span class="spinner-border spinner-border-sm me-2"></span>Creating account...
              </span>
            </button>

            <div class="auth-divider"><span>OR</span></div>

            <a routerLink="/login" class="btn-shopez btn-outline-shopez btn-lg-shopez btn-block">
              Already have an account? Login
            </a>

          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-page { min-height:100vh; background:#f1f3f6; display:flex; align-items:center; justify-content:center; padding:20px; }
    .auth-container { display:grid; grid-template-columns:1fr 1fr; max-width:860px; width:100%; border-radius:12px; overflow:hidden; box-shadow:0 8px 40px rgba(0,0,0,.15); }
    .auth-left { background:linear-gradient(160deg,#2874f0 0%,#0d3b6e 100%); padding:40px 32px; display:flex; align-items:center; }
    .auth-left-content { color:#fff; }
    .auth-brand { font-size:32px; font-weight:800; margin-bottom:8px; }
    .auth-left-tagline { font-size:16px; opacity:.85; margin-bottom:24px; line-height:1.5; }
    .auth-features { list-style:none; padding:0; margin-bottom:24px; }
    .auth-features li { display:flex; align-items:center; gap:10px; font-size:14px; padding:6px 0; }
    .auth-features li i { color:#a5d6a7; font-size:16px; }
    .auth-hero-img { width:100%; height:160px; object-fit:cover; border-radius:8px; opacity:.85; }
    .auth-right { background:#fff; padding:40px 32px; overflow-y:auto; max-height:100vh; }
    .auth-form-title { font-size:22px; font-weight:700; color:#212121; margin-bottom:6px; }
    .auth-form-subtitle { font-size:13px; color:#878787; margin-bottom:24px; }
    .password-wrapper { position:relative; }
    .password-wrapper .shopez-input { padding-right:44px; }
    .password-toggle { position:absolute; right:12px; top:50%; transform:translateY(-50%); background:none; border:none; color:#757575; font-size:16px; cursor:pointer; }
    /* Password strength bar */
    .pwd-strength { display:flex; align-items:center; gap:8px; }
    .pwd-bar { height:4px; border-radius:2px; transition:all .3s; }
    .pwd-label { font-size:11px; font-weight:600; }
    .terms-text { font-size:12px; color:#878787; margin-bottom:16px; line-height:1.6; }
    .terms-text a { color:#2874f0; }
    .auth-divider { display:flex; align-items:center; gap:12px; margin:20px 0; color:#bdbdbd; font-size:13px; }
    .auth-divider::before, .auth-divider::after { content:''; flex:1; height:1px; background:#e0e0e0; }
    @media(max-width:640px) { .auth-container { grid-template-columns:1fr; } .auth-left { display:none; } }
  `]
})
export class RegisterComponent {
  name = ''; email = ''; password = ''; confirmPassword = '';
  loading = false; errorMessage = ''; showPassword = false;
  successMessage: any;

  constructor(private authService: AuthService, private router: Router) {}

  get pwdStrengthWidth(): string {
    if (!this.password) return '0%';
    if (this.password.length < 6)  return '33%';
    if (this.password.length < 10) return '66%';
    return '100%';
  }
  get pwdStrengthColor(): string {
    if (!this.password) return '#e0e0e0';
    if (this.password.length < 6)  return '#e53935';
    if (this.password.length < 10) return '#fb8c00';
    return '#43a047';
  }
  get pwdStrengthLabel(): string {
    if (!this.password) return '';
    if (this.password.length < 6)  return 'Weak';
    if (this.password.length < 10) return 'Moderate';
    return 'Strong';
  }

  register(): void {
    if (!this.name || !this.email || !this.password) { this.errorMessage = 'All fields are required.'; return; }
    if (this.password.length < 6) { this.errorMessage = 'Password must be at least 6 characters.'; return; }
    if (this.password !== this.confirmPassword) { this.errorMessage = 'Passwords do not match.'; return; }

    this.loading = true; this.errorMessage = '';
    this.authService.register({ Name: this.name, Email: this.email, Password: this.password, Role: 'Customer' }).subscribe({
      next: (res) => { this.loading = false; if (res.success) this.router.navigate(['/']); },
      error: (err) => {
        this.loading = false;
        if (err.status === 409)    this.errorMessage = 'An account with this email already exists.';
        else if (err.status === 0) this.errorMessage = 'Cannot connect to server. Is the API running?';
        else                       this.errorMessage = err.error?.message ?? 'Registration failed.';
      }
    });
  }
}