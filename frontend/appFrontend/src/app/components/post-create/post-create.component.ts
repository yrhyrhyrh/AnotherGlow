import { Component, Output, EventEmitter } from '@angular/core';
import { PostService } from '../../services/post.service';
import { CreatePostRequestDTO, PostDTO } from '../../models/dto';
import { MatCardModule } from '@angular/material/card'; // Import MatCardModule
import { MatFormFieldModule } from '@angular/material/form-field'; // Import MatFormFieldModule
import { MatInputModule } from '@angular/material/input'; // Import MatInputModule
import { MatButtonModule } from '@angular/material/button'; // Import MatButtonModule
import { MatIconModule } from '@angular/material/icon';   // Import MatIconModule
import { FormsModule } from '@angular/forms';         // Import FormsModule for ngModel
import { CommonModule } from '@angular/common';          // Import CommonModule
import { AuthService } from '../../services/auth.service';

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
  ],
  templateUrl: './post-create.component.html',
  styleUrls: ['./post-create.component.css']
})
export class PostCreateComponent {
  @Output() postCreated = new EventEmitter<void>();
  postContent = '';
  selectedFiles: FileList | null = null;

  constructor(private postService: PostService, private authService: AuthService) { }

  onFileSelected(event: any): void {
    this.selectedFiles = event.target.files;
  }

  createPost(): void {
    if (this.postContent.trim()) {
      const formData = new FormData();
      formData.append('Content', this.postContent);
      console.log(formData);

      if (this.selectedFiles) {
        for (let i = 0; i < this.selectedFiles.length; i++) {
          formData.append('Attachments', this.selectedFiles[i]);
        }
      }

      this.postService.createPost(formData as any).subscribe( // Type cast to 'any' FormData for now
        (newPost) => {
          this.postContent = '';
          this.selectedFiles = null;
          this.postCreated.emit(); // Emit event to reload posts in parent component
        },
        (error) => {
          console.error('Error creating post:', error);
        }
      );
    }
  }
}