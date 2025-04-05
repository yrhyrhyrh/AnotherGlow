import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private tokenKey = 'jwt_token';

  constructor(private router: Router) {
    console.log('AuthService initialized');
  }

  // Method to retrieve userId from the stored JWT token
  getUserId(): string | null {
    console.log('AuthService - getUserId called');
    const token = localStorage.getItem(this.tokenKey);
    if (!token) {
      console.warn('AuthService - No token found in localStorage');
      return null;
    }

    try {
      console.log('AuthService - Decoding token');
      const payload = JSON.parse(atob(token.split('.')[1]));
      const userId = payload.userId || null;
      console.log('AuthService - Extracted userId:', userId);
      return userId;
    } catch (error) {
      console.error('AuthService - Error decoding token:', error);
      return null;
    }
  }

  // Function to log out the user by removing the token and redirecting to login
  logout(): void {
    console.log('AuthService - logout called');
    localStorage.removeItem(this.tokenKey);
    sessionStorage.removeItem(this.tokenKey); // In case you store it here
    console.log('AuthService - Token removed, redirecting to login page');
    this.router.navigate(['/login']); // Redirect to login page
  }
}
