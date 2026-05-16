import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(private authService: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    // Check if user is logged in
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return false;
    }

    // Check if route requires Admin role
    const requiredRole = route.data['role'];
    if (requiredRole === 'Admin' && !this.authService.isAdmin()) {
      this.router.navigate(['/']);
      return false;
    }

    return true;
  }
}