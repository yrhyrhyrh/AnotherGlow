import { Component, OnInit, inject } from '@angular/core';
import { FormsModule, NgForm, FormControl, FormGroupDirective } from '@angular/forms'; // Import necessary form types
import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { ErrorStateMatcher } from '@angular/material/core';
import { HeaderComponent } from '../header/header.component';
import { UserSettings } from '../interfaces/userSettings';
import { AuthService } from '../services/auth.service';
import { environment } from '../../environments/environment'

interface PasswordChangeData {
  oldPassword: string;
  newPassword: string;
  confirmPassword: string;
}

/** Custom ErrorStateMatcher for password confirmation */
export class ConfirmPasswordErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(control: FormControl | null, form: FormGroupDirective | NgForm | null): boolean {
    const isControlInvalid = !!(control && control.invalid && (control.dirty || control.touched));
    
    const isMismatch = !!(form && form.submitted && (form.form.value.newPassword !== form.form.value.confirmPassword));

     return isControlInvalid;
  }
}


@Component({
  selector: 'app-user-config',
  standalone: true,
  imports: [
    CommonModule,
    HeaderComponent,
    FormsModule,
    RouterModule,
    // --- Angular Material Modules ---
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDividerModule,
    MatIconModule,
    MatSidenavModule,
    MatListModule
    // --- End Angular Material Modules ---
  ],
  templateUrl: './userConfig.component.html',
  styleUrls: ['./userConfig.component.css']
})
export class UserConfigComponent implements OnInit {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  // --- State Properties ---
  isLoading = true;
  isSavingProfile = false;
  isChangingPassword = false;
  userId: string | null = null;
  passwordsDoNotMatch = false; // Flag for password mismatch

  // --- Form Models ---
  user: UserSettings = {
    email: '',
    username: '',
    password: '',
    fullName: '',
    bio: '',
    jobRole: ''
  };

  passwordData: PasswordChangeData = {
    oldPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

  // --- Error Matcher Instance ---
  // confirmPasswordMatcher = new ConfirmPasswordErrorStateMatcher();
  // Simpler approach: We will just rely on the template *ngIf for mismatch error display.
  // The default error state matcher will handle required/minlength etc.

  ngOnInit(): void {
    console.log('UserConfigComponent initialized');
    this.userId = this.authService.getUserId();
    console.log('User ID from AuthService:', this.userId);

    if (!this.userId) {
      console.error('User ID not found. Redirecting to login.');
      this.showSnackbar('Could not identify user. Please log in again.', 'Error');
      this.isLoading = false;
      this.router.navigate(['/login']);
      return;
    }
    this.loadUserProfile();
  }

  loadUserProfile(): void {
    if (!this.userId) return;

    this.isLoading = true;
    const token = this.authService.getToken();
     if (!token) {
        this.handleAuthError("No authentication token found. Please log in.");
        return;
     }
    const headers = new HttpHeaders({ 'Authorization': `Bearer ${token}` });

    this.http.get<any>(`${environment.apiUrl}/api/users/${this.userId}`, { headers })
      .subscribe({
        next: (userData) => {
          console.log('User data received:', userData);
          this.user.email = userData.email;
          this.user.username = userData.username;
          this.user.fullName = userData.fullName ?? '';
          this.user.bio = userData.bio ?? '';
          this.user.jobRole = userData.jobRole ?? '';
          this.isLoading = false;
        },
        error: (err: HttpErrorResponse) => {
          console.error('Error loading user profile:', err);
           if (err.status === 401 || err.status === 403) {
               this.handleAuthError(`Session expired or invalid (${err.status}). Logging out.`);
           } else {
               const message = `Failed to load profile: ${err.statusText} (${err.status})`;
               this.showSnackbar(message, 'Error');
               this.isLoading = false;
           }
        }
      });
  }

  onUpdateProfile(profileForm: NgForm): void {
    if (profileForm.invalid || !this.userId) {
        this.showSnackbar('Please correct the errors in the form.', 'Validation');
        // Mark fields as touched to show errors
        Object.values(profileForm.controls).forEach(control => {
          control.markAsTouched();
        });
        return;
    }
    if (!this.userId) return;

    this.isSavingProfile = true;
    const token = this.authService.getToken();
    if (!token) {
        this.handleAuthError("No authentication token found. Please log in.");
        this.isSavingProfile = false;
        return;
     }
    const headers = new HttpHeaders({
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
    });

    const updatePayload = {
        userId: this.userId,
        email: this.user.email,
        username: this.user.username,
        fullName: this.user.fullName,
        bio: this.user.bio,
        jobRole: this.user.jobRole
    };

    console.log('Submitting profile update:', updatePayload);

    this.http.put(`${environment.apiUrl}/api/users/update/${this.userId}`, updatePayload, { headers })
        .subscribe({
            next: (response) => {
                console.log('Profile update successful:', response);
                this.showSnackbar('Profile updated successfully!', 'Success');
                this.isSavingProfile = false;
                profileForm.form.markAsPristine(); // Reset form state visually
            },
            error: (err: HttpErrorResponse) => {
                console.error('Error updating profile:', err);
                if (err.status === 401 || err.status === 403) {
                    this.handleAuthError(`Session expired or invalid (${err.status}). Logging out.`);
                } else {
                    const message = `Failed to update profile: ${err.error?.message || err.statusText} (${err.status})`;
                    this.showSnackbar(message, 'Error');
                }
                this.isSavingProfile = false;
            }
        });
  }

  onChangePassword(passwordForm: NgForm): void {
    this.passwordsDoNotMatch = false; // Reset flag on submit attempt

    if (passwordForm.invalid || !this.userId) {
        this.showSnackbar('Please fill in all password fields correctly.', 'Validation');
         // Mark fields as touched to show errors
         Object.values(passwordForm.controls).forEach(control => {
          control.markAsTouched();
        });
        return;
    }

    // Check for mismatch *before* submitting to API
    if (this.passwordData.newPassword !== this.passwordData.confirmPassword) {
      this.passwordsDoNotMatch = true; // Set flag for template error message display
      this.showSnackbar('New passwords do not match.', 'Validation');
      // Manually trigger error state appearance on confirmPassword field if needed
      passwordForm.controls['confirmPassword']?.setErrors({'mismatch': true}); // Example of setting manual error
      passwordForm.controls['confirmPassword']?.markAsTouched(); // Ensure it's checked
      return; // Stop submission
    } else {
       // Clear manual mismatch error if they now match
       if (passwordForm.controls['confirmPassword']?.hasError('mismatch')) {
           delete passwordForm.controls['confirmPassword'].errors?.['mismatch'];
           passwordForm.controls['confirmPassword'].updateValueAndValidity();
       }
    }

    if (!this.userId) return; // Should not happen if validation passes, but safe check

    this.isChangingPassword = true;
    const token = this.authService.getToken();
     if (!token) {
        this.handleAuthError("No authentication token found. Please log in.");
        this.isChangingPassword = false;
        return;
     }
    const headers = new HttpHeaders({
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
    });

    const passwordPayload = {
        userId: this.userId,
        oldPassword: this.passwordData.oldPassword,
        newPassword: this.passwordData.newPassword
    };

    console.log('Submitting password change request...');

    this.http.post(`${environment.apiUrl}/api/users/change-password`, passwordPayload, { headers })
        .subscribe({
            next: (response) => {
                console.log('Password change successful:', response);
                this.showSnackbar('Password changed successfully!', 'Success');
                passwordForm.resetForm(); // Reset form state and controls
                this.passwordData = { oldPassword: '', newPassword: '', confirmPassword: '' };
                this.passwordsDoNotMatch = false; // Reset flag on success
                this.isChangingPassword = false;
            },
            error: (err: HttpErrorResponse) => {
                console.error('Error changing password:', err);
                 if (err.status === 401 || err.status === 403) {
                     this.handleAuthError(`Session expired or invalid (${err.status}). Logging out.`);
                 } else {
                     // Try specific backend message first, fallback to generic
                     const message = `Failed to change password: ${err.error?.message || err.error || err.statusText} (${err.status})`;
                     this.showSnackbar(message, 'Error', 5000);
                 }
                // Clear only old password on error? Or all? Let's clear old.
                this.passwordData.oldPassword = '';
                passwordForm.controls['oldPassword'].markAsPristine();
                this.isChangingPassword = false;
            }
        });
  }

  // Helper to show snackbar messages
  private showSnackbar(message: string, type: 'Success' | 'Error' | 'Validation' | 'Info' = 'Info', duration: number = 3000): void {
    this.snackBar.open(message, 'Close', {
      duration: duration,
      verticalPosition: 'top',
      horizontalPosition: 'center',
      panelClass: type === 'Error' ? ['snackbar-error'] : type === 'Success' ? ['snackbar-success'] : ['snackbar-info']
    });
  }

   // Helper to handle Auth Errors consistently
   private handleAuthError(message: string): void {
        this.showSnackbar(message, 'Error', 5000);
        this.isLoading = false; // Ensure loading state is off
        this.isSavingProfile = false;
        this.isChangingPassword = false;
        this.authService.logout(); // Use the AuthService logout method
   }

   // Method needed for the logout button in the template
   logout(): void {
     this.authService.logout();
   }
}