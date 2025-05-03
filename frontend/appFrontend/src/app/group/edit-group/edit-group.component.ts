import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { GroupService } from '../../services/group.service';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-edit-group',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule
  ],
  templateUrl: './edit-group.component.html',
  styleUrl: './edit-group.component.css'
})
export class EditGroupComponent implements OnInit {
  editGroupForm: FormGroup;
  groupId: string | null = null;
  isLoading = false;
  currentImageUrl: string | null = null;
  selectedFile: File | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private formBuilder: FormBuilder,
    private groupService: GroupService,
    private snackBar: MatSnackBar
  ) {
    this.editGroupForm = this.formBuilder.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      description: [''],
      groupPicture: [null]
    });
  }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      this.groupId = params.get('id');
      if (this.groupId) {
        this.loadGroupDetails();
      }
    });
  }

  loadGroupDetails() {
    this.isLoading = true;
    this.groupService.getGroupDetails(this.groupId!).subscribe({
      next: (data) => {
        this.editGroupForm.patchValue({
          name: data.Name,
          description: data.Description
        });
        this.currentImageUrl = data.GroupPictureUrl || null;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading group details:', error);
        this.snackBar.open('Failed to load group details', 'Close', {
          duration: 3000
        });
        this.isLoading = false;
      }
    });
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
      this.editGroupForm.patchValue({
        groupPicture: this.selectedFile
      });
    }
  }

  onSubmit() {
    if (this.editGroupForm.invalid || !this.groupId) {
      return;
    }

    this.isLoading = true;
    const formData = new FormData();
    formData.append('Name', this.editGroupForm.get('name')?.value);
    formData.append('Description', this.editGroupForm.get('description')?.value);
    if (this.selectedFile) {
      formData.append('GroupPicture', this.selectedFile);
    }
    formData.append('GroupId', this.groupId!);

    this.groupService.updateGroup(this.groupId!, formData).subscribe({
      next: () => {
        this.snackBar.open('Group updated successfully', 'Close', {
          duration: 3000
        });
        this.router.navigate(['/group/detail', this.groupId]);
      },
      error: (error) => {
        console.error('Error updating group:', error);
        this.snackBar.open('Failed to update group', 'Close', {
          duration: 3000
        });
        this.isLoading = false;
      }
    });
  }

  goBack() {
    this.router.navigate(['/group/detail', this.groupId]);
  }
} 