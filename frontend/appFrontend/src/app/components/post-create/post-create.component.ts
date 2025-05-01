import { Component, Output, EventEmitter, Input, ViewChild, AfterViewInit } from '@angular/core';
import { PostService } from '../../services/post.service';
import { CreatePostRequestDTO, PostDTO } from '../../models/postDto';
import { MatCardModule } from '@angular/material/card'; // Import MatCardModule
import { MatFormFieldModule } from '@angular/material/form-field'; // Import MatFormFieldModule
import { MatInputModule } from '@angular/material/input'; // Import MatInputModule
import { MatButtonModule } from '@angular/material/button'; // Import MatButtonModule
import { MatIconModule } from '@angular/material/icon';   // Import MatIconModule
import { FormsModule } from '@angular/forms';         // Import FormsModule for ngModel
import { CommonModule } from '@angular/common';          // Import CommonModule
import { AuthService } from '../../services/auth.service';
import { PollingComponent } from '../../polling/polling.component';

@Component({
  selector: 'app-post-create',
  standalone: true,
  imports: [
    MatCardModule,         // Add MatCardModule to imports
    MatFormFieldModule,    // Add MatFormFieldModule
    MatInputModule,       // Add MatInputModule
    MatButtonModule,      // Add MatButtonModule
    MatIconModule,        // Add MatIconModule
    FormsModule,          // Add FormsModule
    CommonModule,           // Add CommonModule
    PollingComponent
  ],
  templateUrl: './post-create.component.html',
  styleUrls: ['./post-create.component.css']
})
export class PostCreateComponent implements AfterViewInit {
  @Output() postCreated = new EventEmitter<void>();
  @Input() groupId: string | null = null;
  postContent = '';
  selectedFiles: FileList | null = null;
  isPoll = false;  // Poll flag

  @ViewChild(PollingComponent) pollingComponent!: PollingComponent; // Access PollingComponent

  constructor(private postService: PostService) { }

  ngAfterViewInit() {
    // Ensure the pollingComponent is available after view initialization
    if (this.pollingComponent) {
      console.log('PollingComponent is initialized:', this.pollingComponent);
    } else {
      console.error('PollingComponent is not available');
    }
  }
  togglePoll(): void {
    this.isPoll = !this.isPoll;
  }
  onFileSelected(event: any): void {
    this.selectedFiles = event.target.files;
  }

  createPost(): void {
    if (this.postContent.trim()) {
      const formData = new FormData();
      formData.append('Content', this.postContent);
      formData.append('GroupId', this.groupId ?? '');

      // If the post includes a poll, add the poll data
      if (this.isPoll && this.pollingComponent && this.pollingComponent.pollForm.valid) {
        const pollData = this.pollingComponent.pollForm.value;  // Get poll data
        formData.append('Poll', JSON.stringify(pollData)); // Add poll data as JSON string
      }

      // Handle files if they exist
      if (this.selectedFiles) {
        for (let i = 0; i < this.selectedFiles.length; i++) {
          formData.append('Attachments', this.selectedFiles[i]);
        }
      }

      // Make the API call to create the post
      this.postService.createPost(formData as any).subscribe(
        (newPost) => {
          this.postContent = '';
          this.selectedFiles = null;
          this.isPoll = false;
          this.postCreated.emit(); // Emit event to reload posts
        },
        (error) => {
          console.error('Error creating post:', error);
        }
      );
    }
  }
}
