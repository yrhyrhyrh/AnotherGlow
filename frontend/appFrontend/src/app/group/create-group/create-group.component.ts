import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { jwtDecode } from "jwt-decode";

interface GroupRequest {
  Name: string;
  Description: string;
  UserId: string;
}

@Component({
  selector: 'app-create-group',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <div class="create-group-container">
      <h2>Create New Group</h2>
      <form (ngSubmit)="createGroup()" #groupForm="ngForm">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Group Name</mat-label>
          <input matInput [(ngModel)]="groupRequest.Name" name="name" required>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Description</mat-label>
          <textarea matInput [(ngModel)]="groupRequest.Description" name="description" rows="4"></textarea>
        </mat-form-field>

        <div class="button-container">
          <button mat-button type="button" (click)="goBack()">
            <mat-icon>arrow_back</mat-icon>
            Back
          </button>
          <button mat-raised-button color="primary" type="submit" [disabled]="!groupRequest.Name.trim()">
            <mat-icon>add</mat-icon>
            Create Group
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .create-group-container {
      max-width: 600px;
      margin: 2rem auto;
      padding: 2rem;
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }

    h2 {
      margin-bottom: 2rem;
      color: #333;
    }

    .full-width {
      width: 100%;
      margin-bottom: 1rem;
    }

    .button-container {
      display: flex;
      justify-content: space-between;
      margin-top: 2rem;
    }

    mat-form-field {
      width: 100%;
    }
  `]
})
export class CreateGroupComponent implements OnInit {
  groupRequest: GroupRequest = {
    Name: '',
    Description: '',
    UserId: ''
  };

  constructor(private http: HttpClient, private router: Router) {
    const token = localStorage.getItem('jwt_token');
    if (token) {
      try {
        const decoded: any = jwtDecode(token);
        this.groupRequest.UserId = decoded.userId || "";
      } catch (error) {
        console.error("Invalid token:", error);
      }
    } else {
      console.warn("No JWT token found in localStorage");
    }
  }

  ngOnInit(): void { }

  createGroup() {
    if (this.groupRequest.Name.trim()) {
      this.http.post<{ token: string }>('http://localhost:5181/api/group/create', this.groupRequest)
        .subscribe({
          next: (response) => {
            console.log('Group created successfully:', response);
            this.router.navigate(['/groups/general/posts']);
          },
          error: (error) => {
            console.error("Request error:", error);
          }
        });
    }
  }

  goBack() {
    this.router.navigate(['/groups/general/posts']);
  }
} 