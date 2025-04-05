import { Component } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { HttpClient } from '@angular/common/http';
import { RouterModule, Router } from '@angular/router';
import { User } from "../interfaces/user";

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
  
  onSubmit() {
    const { username, password } = this.user;

    if (!username || !password) {
      console.error("Username and Password are required.");
      return;
    }

    this.http.post<{ token: string; userId: string; fullName?: string }>(
      'http://localhost:5181/api/auth/login',
      this.user
    ).subscribe({
      next: (response) => {
        console.log("Login successful:", response.token);

        if (typeof window !== "undefined") {
          localStorage.setItem('jwt_token', response.token);
          localStorage.setItem('userId', response.userId); // ✅ Save userId
          if (response.fullName) {
            localStorage.setItem('fullName', response.fullName); // Optional
          }
        }

        this.router.navigate(['/']);
      },
      error: (error) => {
        console.error("Login error:", error);
      }
    });
  }
}
