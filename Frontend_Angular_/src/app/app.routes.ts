import { Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';

export const routes: Routes = [
  // Public routes
  { path: '', loadComponent: () => import('./components/home/home.component').then(m => m.HomeComponent) },
  { path: 'products', loadComponent: () => import('./components/products/products.component').then(m => m.ProductsComponent) },
  { path: 'products/:id', loadComponent: () => import('./components/product-detail/product-detail.component').then(m => m.ProductDetailComponent) },
  { path: 'login', loadComponent: () => import('./components/login/login.component').then(m => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./components/register/register.component').then(m => m.RegisterComponent) },
  { path: 'cart', loadComponent: () => import('./components/cart/cart.component').then(m => m.CartComponent) },

  // Protected routes (require login)
  {
    path: 'checkout',
    loadComponent: () => import('./components/checkout/checkout.component').then(m => m.CheckoutComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'order-success/:id',
    loadComponent: () => import('./components/order-success/order-success.component').then(m => m.OrderSuccessComponent),
    canActivate: [AuthGuard]
  },

  // Admin routes (require Admin role)
  {
    path: 'admin',
    loadComponent: () => import('./components/admin/admin.component').then(m => m.AdminComponent),
    canActivate: [AuthGuard],
    data: { role: 'Admin' }
  },

  // Fallback
  { path: '**', redirectTo: '' }
];