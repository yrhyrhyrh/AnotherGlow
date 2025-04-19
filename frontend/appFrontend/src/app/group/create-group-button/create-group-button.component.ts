import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';

@Component({
  selector: 'app-create-group-button',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <button mat-raised-button color="primary" (click)="createGroup()" class="create-group-button">
      <mat-icon>add</mat-icon>
      Create Group
    </button>
  `,
  styles: [`
    .create-group-button {
      margin: 16px;
      width: calc(100% - 32px);
    }
  `]
})
export class CreateGroupButtonComponent {
  constructor(private router: Router) { }

  createGroup() {
    this.router.navigate(['/group/create']);
  }
} 