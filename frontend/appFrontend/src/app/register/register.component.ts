import { Component } from "@angular/core";
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { User } from "../interfaces/user";
import { HttpClient } from "@angular/common/http";
import { Router, RouterModule } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: "app-register",
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
  templateUrl: "./register.component.html",
  styleUrls: ["./register.component.css"]
})
export class RegisterComponent {
  registerForm: FormGroup;
  hidePassword = true;
  isLoading = false;

  constructor(
    private http: HttpClient,
    private router: Router,
    private formBuilder: FormBuilder,
    private snackBar: MatSnackBar
  ) {
    this.registerForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      username: ['', [Validators.required, Validators.minLength(3)]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSubmit() {
    if (this.registerForm.invalid) {
      return;
    }

    this.isLoading = true;
    const { email, username, password } = this.registerForm.value;

    this.http.post<{ token: string }>('http://localhost:5181/api/auth/register', { email, username, password })
      .subscribe({
        next: (response) => {
          console.log("Register successful:", response.token);
          if (typeof window !== "undefined") {
            localStorage.setItem('jwt_token', response.token);
          }

          this.snackBar.open('Registration successful!', 'Close', {
            duration: 3000,
            horizontalPosition: 'end',
            verticalPosition: 'top'
          });

          this.router.navigate(['/']);
        },
        error: (error) => {
          console.error("Registration error:", error);
          this.snackBar.open('Registration failed. Please try again.', 'Close', {
            duration: 3000,
            horizontalPosition: 'end',
            verticalPosition: 'top'
          });
          this.isLoading = false;
        }
      });
  }

  getErrorMessage(field: string): string {
    if (this.registerForm.get(field)?.hasError('required')) {
      return 'This field is required';
    }
    if (field === 'email' && this.registerForm.get(field)?.hasError('email')) {
      return 'Please enter a valid email address';
    }
    if (field === 'username' && this.registerForm.get(field)?.hasError('minlength')) {
      return 'Username must be at least 3 characters';
    }
    if (field === 'password' && this.registerForm.get(field)?.hasError('minlength')) {
      return 'Password must be at least 6 characters';
    }
    return '';
  }
}
