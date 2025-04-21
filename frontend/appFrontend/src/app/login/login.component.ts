import { Component } from "@angular/core";
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { HttpClient } from '@angular/common/http';
import { RouterModule, Router } from '@angular/router';
import { User } from "../interfaces/user";
import { environment } from '../../environments/environment';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: "app-login",
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatSnackBarModule,
    MatProgressSpinnerModule
  ],
  templateUrl: "./login.component.html",
  styleUrls: ["./login.component.css"]
})
export class LoginComponent {
  loginForm: FormGroup;
  hidePassword = true;
  isLoading = false;

  private baseUrl = environment.apiUrl;
  private apiUrl = `${this.baseUrl}/api/auth/login`;

  constructor(
    private http: HttpClient,
    private router: Router,
    private formBuilder: FormBuilder,
    private snackBar: MatSnackBar
  ) {
    this.loginForm = this.formBuilder.group({
      username: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSubmit() {
    if (this.loginForm.invalid) {
      return;
    }

    this.isLoading = true;
    const { username, password } = this.loginForm.value;

    this.http.post<{ token: string; userId: string; fullName?: string }>(
      this.apiUrl,
      { username, password }
    ).subscribe({
      next: (response) => {
        console.log("Login successful:", response.token);

        if (typeof window !== "undefined") {
          localStorage.setItem('jwt_token', response.token);
          localStorage.setItem('userId', response.userId);
          if (response.fullName) {
            localStorage.setItem('fullName', response.fullName);
          }
        }

        this.snackBar.open('Login successful!', 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top'
        });

        this.router.navigate(['/']);
      },
      error: (error) => {
        console.error("Login error:", error);
        this.snackBar.open('Login failed. Please check your credentials.', 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top'
        });
        this.isLoading = false;
      }
    });
  }

  getErrorMessage(field: string): string {
    if (this.loginForm.get(field)?.hasError('required')) {
      return 'This field is required';
    }
    if (field === 'password' && this.loginForm.get(field)?.hasError('minlength')) {
      return 'Password must be at least 6 characters';
    }
    return '';
  }
}
