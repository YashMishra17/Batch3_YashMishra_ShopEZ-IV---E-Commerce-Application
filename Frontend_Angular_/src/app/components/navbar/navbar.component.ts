import { Component, OnInit, HostListener } from '@angular/core';
import { RouterLink, RouterLinkActive, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { CartService } from '../../services/cart.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, CommonModule, FormsModule],
  template: `
    <!-- Top bar (Amazon style) -->
    <div class="navbar-top">
      <div class="container-shopez d-flex align-items-center gap-3" style="padding:10px 16px;">

        <!-- Logo -->
        <a routerLink="/" class="navbar-brand-shopez">
          <span class="brand-shop">Shop</span><span class="brand-ez">EZ</span>
          <small class="brand-tagline">.in</small>
        </a>

        <!-- Search bar (Flipkart style) -->
        <div class="search-bar flex-fill">
          <div class="search-inner">
            <input
              type="text"
              class="search-input"
              placeholder="Search for products, brands and more"
              [(ngModel)]="searchQuery"
              (keyup.enter)="search()"
            />
            <button class="search-btn" (click)="search()">
              <i class="bi bi-search"></i>
            </button>
          </div>
        </div>

        <!-- Right nav -->
        <div class="nav-right d-flex align-items-center gap-3">

          <!-- Login / User -->
          <div class="nav-btn-group" *ngIf="!isLoggedIn">
            <a routerLink="/login" class="nav-top-link">
              <span class="nav-top-label">Hello, Sign In</span>
              <span class="nav-top-value">Account <i class="bi bi-chevron-down small"></i></span>
            </a>
          </div>

          <div class="dropdown" *ngIf="isLoggedIn">
            <div class="nav-top-link dropdown-toggle" style="cursor:pointer" data-bs-toggle="dropdown">
              <span class="nav-top-label">Hello, {{ userName }}</span>
              <span class="nav-top-value">Account <i class="bi bi-chevron-down small"></i></span>
            </div>
            <ul class="dropdown-menu dropdown-menu-end">
              <li><a class="dropdown-item" routerLink="/admin" *ngIf="isAdmin"><i class="bi bi-gear me-2"></i>Admin Panel</a></li>
              <li><hr class="dropdown-divider" *ngIf="isAdmin"></li>
              <li><button class="dropdown-item text-danger" (click)="logout()"><i class="bi bi-box-arrow-right me-2"></i>Logout</button></li>
            </ul>
          </div>

          <!-- Cart -->
          <a routerLink="/cart" class="nav-cart-btn">
            <div class="cart-icon-wrapper">
              <i class="bi bi-cart3" style="font-size:22px;"></i>
              <span class="cart-count-badge" *ngIf="cartCount > 0">{{ cartCount }}</span>
            </div>
            <span class="nav-top-value">Cart</span>
          </a>

        </div>
      </div>
    </div>

    <!-- Bottom nav bar (category links) -->
    <div class="navbar-bottom">
      <div class="container-shopez">
        <nav class="bottom-nav">
          <a routerLink="/" routerLinkActive="active-nav" [routerLinkActiveOptions]="{exact:true}" class="bottom-nav-link">
            <i class="bi bi-house"></i> Home
          </a>
          <a routerLink="/products" routerLinkActive="active-nav" class="bottom-nav-link">
            <i class="bi bi-grid"></i> All Products
          </a>
          <a routerLink="/products" [queryParams]="{q:'laptop'}" class="bottom-nav-link">
            <i class="bi bi-laptop"></i> Laptops
          </a>
          <a routerLink="/products" [queryParams]="{q:'mobile'}" class="bottom-nav-link">
            <i class="bi bi-phone"></i> Mobiles
          </a>
          <a routerLink="/products" [queryParams]="{q:'keyboard'}" class="bottom-nav-link">
            <i class="bi bi-keyboard"></i> Accessories
          </a>
          <a routerLink="/login" class="bottom-nav-link" *ngIf="!isLoggedIn">
            <i class="bi bi-person"></i> Login
          </a>
          <a routerLink="/register" class="bottom-nav-link" *ngIf="!isLoggedIn">
            <i class="bi bi-person-plus"></i> Register
          </a>
          <a routerLink="/admin" class="bottom-nav-link" *ngIf="isAdmin">
            <i class="bi bi-gear"></i> Admin
          </a>
        </nav>
      </div>
    </div>
  `,
  styles: [`
    /* Top bar */
    .navbar-top {
      background: linear-gradient(135deg, #131921 0%, #1a2332 100%);
      position: sticky; top: 0; z-index: 1000;
      box-shadow: 0 2px 8px rgba(0,0,0,.3);
    }
    .navbar-brand-shopez {
      white-space: nowrap; text-decoration: none;
      display: flex; align-items: baseline; gap: 0;
    }
    .brand-shop { color: #ff9f00; font-size: 24px; font-weight: 800; letter-spacing: -1px; }
    .brand-ez   { color: #fff;    font-size: 24px; font-weight: 800; letter-spacing: -1px; }
    .brand-tagline { color: #ff9f00; font-size: 11px; font-weight: 500; }

    /* Search */
    .search-bar { max-width: 600px; min-width: 200px; }
    .search-inner { display: flex; border-radius: 4px; overflow: hidden; box-shadow: 0 0 0 2px #ff9f00; }
    .search-input {
      flex: 1; padding: 10px 14px;
      border: none; font-size: 14px;
      outline: none; min-width: 0;
    }
    .search-btn {
      background: #ff9f00; border: none;
      padding: 0 18px; font-size: 18px;
      color: #111; transition: background .2s;
    }
    .search-btn:hover { background: #e68900; }

    /* Nav right links */
    .nav-top-link {
      display: flex; flex-direction: column;
      color: #fff; text-decoration: none;
      padding: 4px 8px; border-radius: 4px;
      transition: background .15s;
      cursor: pointer;
    }
    .nav-top-link:hover { background: rgba(255,255,255,.1); }
    .nav-top-label { font-size: 11px; color: #ccc; }
    .nav-top-value { font-size: 13px; font-weight: 700; color: #fff; }

    /* Cart */
    .nav-cart-btn {
      display: flex; flex-direction: column; align-items: center;
      color: #fff; text-decoration: none;
      padding: 4px 8px; border-radius: 4px;
      transition: background .15s;
      position: relative;
    }
    .nav-cart-btn:hover { background: rgba(255,255,255,.1); }
    .cart-icon-wrapper { position: relative; }
    .cart-count-badge {
      position: absolute; top: -8px; right: -10px;
      background: #ff9f00; color: #111;
      border-radius: 50%; width: 20px; height: 20px;
      font-size: 11px; font-weight: 800;
      display: flex; align-items: center; justify-content: center;
    }

    /* Bottom nav */
    .navbar-bottom {
      background: #232f3e;
      padding: 0;
    }
    .bottom-nav {
      display: flex; align-items: center;
      overflow-x: auto; gap: 0;
    }
    .bottom-nav::-webkit-scrollbar { display: none; }
    .bottom-nav-link {
      display: flex; align-items: center; gap: 5px;
      color: #ddd; text-decoration: none;
      padding: 10px 16px; font-size: 13px; font-weight: 500;
      white-space: nowrap;
      border-bottom: 2px solid transparent;
      transition: all .2s;
    }
    .bottom-nav-link:hover  { color: #fff; background: rgba(255,255,255,.08); border-bottom-color: #ff9f00; }
    .active-nav { color: #fff !important; border-bottom-color: #ff9f00 !important; }

    /* Dropdown */
    .dropdown-menu { min-width: 180px; border: none; box-shadow: 0 4px 20px rgba(0,0,0,.15); }
    .dropdown-item { font-size: 14px; padding: 10px 16px; }
    .dropdown-item:hover { background: #f5f5f5; }
  `]
})
export class NavbarComponent implements OnInit {
  isLoggedIn = false;
  isAdmin = false;
  userName = '';
  cartCount = 0;
  searchQuery = '';

  constructor(
    private authService: AuthService,
    private cartService: CartService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.isLoggedIn = !!user;
      this.isAdmin = user?.Role === 'Admin';
      this.userName = user?.Name?.split(' ')[0] ?? '';
    });
    this.cartService.cart$.subscribe(() => {
      this.cartCount = this.cartService.getCartCount();
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/']);
  }

  search(): void {
    if (this.searchQuery.trim()) {
      this.router.navigate(['/products'], { queryParams: { q: this.searchQuery.trim() } });
    }
  }
}