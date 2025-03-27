import { Component } from "@angular/core";
import { FormsModule } from "@angular/forms";  // Import FormsModule
import { HttpClient, withFetch } from '@angular/common/http'; // Import HttpClient
import { RouterModule, Router } from '@angular/router'; // Import RouterModule for routing
import { User } from "../interfaces/user"; // Your User interface

@Component({
  selector: "app-login",
  standalone: true,
  imports: [FormsModule, RouterModule],
  templateUrl: "./login.component.html",
  styleUrls: ["./login.component.css"]
})
export class LoginComponent {
  user: User = {
    username: "",
    password: "",
  };

  constructor(private http: HttpClient, private router: Router) {}
  
  // Method to handle login form submission
  onSubmit() {
    const { username, password } = this.user;

    if (!username || !password) {
      console.error("Username and Password are required.");
      return;
    }

    // Send POST request to backend API
    this.http.post<{ token: string }>('http://localhost:5181/api/auth/login', this.user)
      .subscribe({
        next: (response) => {
          console.log("Login successful:", response.token);
          // Store the token in localStorage or sessionStorage
          if (typeof window !== "undefined") {
            localStorage.setItem('jwt_token', response.token);
          }
          // Redirect to home after successful login
          this.router.navigate(['/']);
        },
        error: (error) => {
          console.error("Login error:", error);
        }
      });
  }
}
