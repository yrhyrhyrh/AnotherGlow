import { Component } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { User } from "../interfaces/user";
import { HttpClient } from "@angular/common/http";
import { Router, RouterModule } from '@angular/router';
import { environment } from "../../environments/environment";

@Component({
  selector: "app-register",
  standalone: true,
  imports: [FormsModule, RouterModule],
  templateUrl: "./register.component.html",
  styleUrls: ["./register.component.css"]
})
export class RegisterComponent {
  user: User = {
    email: "",
    username: "",
    password: "",
  }

  constructor(private http: HttpClient, private router: Router) {}
  
  // Method to handle login form submission
  onSubmit() {
    const { email, username, password } = this.user;

    if (!username || !password || !email) {
      console.error("Email, Username and Password are required.");
      return;
    }

    // Send POST request to backend API
    this.http.post<{ token: string }>(`${environment.apiUrl}/api/auth/register`, this.user)
      .subscribe({
        next: (response) => {
          console.log("Register successful:", response.token);
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
