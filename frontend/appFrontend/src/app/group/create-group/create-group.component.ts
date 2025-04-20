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
  GroupPicture?: File;
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
      <form (ngSubmit)="createGroup()" #groupForm="ngForm" enctype="multipart/form-data">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Group Name</mat-label>
          <input matInput [(ngModel)]="groupRequest.Name" name="name" required>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Description</mat-label>
          <textarea matInput [(ngModel)]="groupRequest.Description" name="description" rows="4"></textarea>
        </mat-form-field>

        <div class="file-upload-container">
          <input type="file" 
                 (change)="onFileSelected($event)" 
                 accept="image/jpeg,image/png,image/gif"
                 #fileInput
                 style="display: none">
          <button type="button" 
                  mat-stroked-button 
                  (click)="fileInput.click()"
                  class="full-width">
            <mat-icon>add_photo_alternate</mat-icon>
            {{ groupRequest.GroupPicture ? 'Change Group Picture' : 'Add Group Picture' }}
          </button>
          <div *ngIf="groupRequest.GroupPicture" class="selected-file">
            <mat-icon>image</mat-icon>
            <span>{{ groupRequest.GroupPicture.name }}</span>
            <button mat-icon-button (click)="removeFile()">
              <mat-icon>close</mat-icon>
            </button>
          </div>
        </div>

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

    .file-upload-container {
      margin-bottom: 1rem;
    }

    .selected-file {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-top: 8px;
      padding: 8px;
      background: #f5f5f5;
      border-radius: 4px;
    }

    .selected-file span {
      flex: 1;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
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

  onFileSelected(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (file) {
      if (file.size > 5 * 1024 * 1024) { // 5MB limit
        alert('File size too large. Maximum size: 5MB');
        return;
      }
      this.groupRequest.GroupPicture = file;
    }
  }

  removeFile() {
    this.groupRequest.GroupPicture = undefined;
  }

  createGroup() {
    if (this.groupRequest.Name.trim()) {
      const formData = new FormData();
      formData.append('Name', this.groupRequest.Name);
      formData.append('Description', this.groupRequest.Description || '');
      formData.append('UserId', this.groupRequest.UserId);
      if (this.groupRequest.GroupPicture) {
        formData.append('GroupPicture', this.groupRequest.GroupPicture);
      }

      this.http.post<{ token: string }>('http://localhost:5181/api/group/create', formData)
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