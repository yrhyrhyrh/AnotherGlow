import { Component, Input } from '@angular/core';
import { PostDTO, CommentDTO } from '../../models/dto';
import { SocialActionsService } from '../../services/social-actions.service';
import { MatCardModule } from '@angular/material/card'; // Import MatCardModule
import { MatFormFieldModule } from '@angular/material/form-field'; // Import MatFormFieldModule
import { MatInputModule } from '@angular/material/input'; // Import MatInputModule
import { MatButtonModule } from '@angular/material/button'; // Import MatButtonModule
import { MatIconModule } from '@angular/material/icon';   // Import MatIconModule
import { FormsModule } from '@angular/forms';         // Import FormsModule for ngModel
import { CommonModule } from '@angular/common';          // Import CommonModule
import { CommentListComponent } from '../comment-list/comment-list.component';
import { CommentCreateComponent } from '../comment-create/comment-create.component';

@Component({
  selector: 'app-post-card',
  standalone: true,
  imports: [
    MatCardModule,         // Add MatCardModule to imports
    MatFormFieldModule,    // Add MatFormFieldModule
    MatInputModule,       // Add MatInputModule
    MatButtonModule,      // Add MatButtonModule
    MatIconModule,        // Add MatIconModule
    FormsModule,          // Add FormsModule
    CommonModule,           // Add CommonModule
    CommentListComponent,
    CommentCreateComponent
  ],
  templateUrl: './post-card.component.html',
  styleUrls: ['./post-card.component.css']
})
export class PostCardComponent {
  @Input() post!: PostDTO;
  isLiked = false; // Placeholder - Implement actual like status check later
  commentsVisible = false;

  constructor(private socialActionsService: SocialActionsService) { }

  likePost(): void {
    this.socialActionsService.likePost(this.post.PostId).subscribe(
      (like) => {
        this.isLiked = true; // Update like status
        this.post.LikeCount++; // Update like count
        // Optionally update LikeId if needed
      },
      (error) => {
        console.error('Error liking post:', error);
        // Handle error (e.g., already liked, post not found)
      }
    );
  }

  unlikePost(): void {
    this.socialActionsService.unlikePost(this.post.PostId).subscribe(
      () => {
        this.isLiked = false; // Update like status
        this.post.LikeCount--; // Update like count
      },
      (error) => {
        console.error('Error unliking post:', error);
        // Handle error
      }
    );
  }

  toggleComments(): void {
    this.commentsVisible = !this.commentsVisible;
  }

  onCommentAdded(newComment: CommentDTO): void {
    this.post.CommentCount++;
    // Optionally update the comments list if you are displaying comments in post-card
    // For now, we will reload the whole post list in post-list component for simplicity
  }
}