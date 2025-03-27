import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private tokenKey = 'jwt_token';

  constructor(private router: Router) {}

  // Function to remove token
  logout(): void {
    localStorage.removeItem(this.tokenKey);
    sessionStorage.removeItem(this.tokenKey); // In case you store it here
    this.router.navigate(['/login']); // Redirect to login page
  }
}