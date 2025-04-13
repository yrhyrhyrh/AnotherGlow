import { Component, Input, Output, EventEmitter } from '@angular/core';
import { SocialActionsService } from '../../services/social-actions.service';
import { CreateCommentRequestDTO, CommentDTO } from '../../models/postDto';
import { MatCardModule } from '@angular/material/card'; // Import MatCardModule
import { MatFormFieldModule } from '@angular/material/form-field'; // Import MatFormFieldModule
import { MatInputModule } from '@angular/material/input'; // Import MatInputModule
import { MatButtonModule } from '@angular/material/button'; // Import MatButtonModule
import { MatIconModule } from '@angular/material/icon';   // Import MatIconModule
import { FormsModule } from '@angular/forms';         // Import FormsModule for ngModel
import { CommonModule } from '@angular/common';          // Import CommonModule

@Component({
  selector: 'app-comment-create',
  standalone: true,
  imports: [
    MatCardModule,         // Add MatCardModule to imports
    MatFormFieldModule,    // Add MatFormFieldModule
    MatInputModule,       // Add MatInputModule
    MatButtonModule,      // Add MatButtonModule
    MatIconModule,        // Add MatIconModule
    FormsModule,          // Add FormsModule
    CommonModule           // Add CommonModule
  ],
  templateUrl: './comment-create.component.html',
  styleUrls: ['./comment-create.component.css']
})
export class CommentCreateComponent {
  @Input() PostId!: string;
  @Output() commentCreated = new EventEmitter<CommentDTO>();
  commentContent = '';

  constructor(private socialActionsService: SocialActionsService) { }

  addComment(): void {
    if (this.commentContent.trim()) {
      const commentRequest: CreateCommentRequestDTO = { Content: this.commentContent };
      this.socialActionsService.addCommentToPost(this.PostId, commentRequest).subscribe(
        (newComment) => {
          this.commentCreated.emit(newComment); // Emit the new comment
          this.commentContent = ''; // Clear input
        },
        (error) => {
          console.error('Error adding comment:', error);
        }
      );
    }
  }
}