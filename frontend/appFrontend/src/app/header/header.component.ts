import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-header',
  imports: [MatIconModule, MatButtonModule],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent {
  constructor(private router: Router) { }

  logout() {
    localStorage.removeItem('jwt_token'); // Remove token from storage
    this.router.navigate(['/login']); // Redirect to login page
  }

  goToRoot() {
    this.router.navigate(['/']);
  }

  goToSettings() {
    this.router.navigate(['/userSettings']);
  }
}
