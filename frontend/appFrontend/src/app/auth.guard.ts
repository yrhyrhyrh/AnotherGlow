import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root',
})
export class AuthGuard implements CanActivate {
  constructor(private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    const token = localStorage.getItem('jwt_token');

    if (token && this.isTokenValid(token)) {
      return true;
    }

    this.router.navigate(['/login']); // Redirect to login page if token is invalid
    return false;
  }

  private isTokenValid(token: string): boolean {
    try {
      const decodedToken: any = jwtDecode(token);
      const expiryTime = decodedToken.exp * 1000; // Convert to milliseconds
      return Date.now() < expiryTime; // Check if token is still valid
    } catch (error) {
      return false; // Invalid token
    }
  }
}
