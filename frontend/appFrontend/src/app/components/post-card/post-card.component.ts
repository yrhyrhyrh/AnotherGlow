// post-card.component.ts (Updated)
import { Component, Input, ViewChild } from '@angular/core';
import { PostDTO, CommentDTO, AttachmentDTO } from '../../models/postDto'; // Make sure to import AttachmentDTO
import { SocialActionsService } from '../../services/social-actions.service';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { CommentListComponent } from '../comment-list/comment-list.component';
import { CommentCreateComponent } from '../comment-create/comment-create.component';
import { PollComponent } from '../../poll-list/poll.component';

@Component({
  selector: 'app-post-card',
  standalone: true,
  imports: [
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    FormsModule,
    CommonModule,
    CommentListComponent,
    CommentCreateComponent,
    PollComponent
  ],
  templateUrl: './post-card.component.html',
  styleUrls: ['./post-card.component.css']
})
export class PostCardComponent {
  @Input() post!: PostDTO;
  commentsVisible = false;
  @ViewChild(CommentListComponent) commentList!: CommentListComponent;

  constructor(private socialActionsService: SocialActionsService) { }

  likePost(): void {
    this.socialActionsService.likePost(this.post.PostId).subscribe(
      (like) => {
        this.post.IsLikedByCurrentUser = true;
        this.post.LikeCount++;
      },
      (error) => {
        console.error('Error liking post:', error);
      }
    );
  }

  unlikePost(): void {
    this.socialActionsService.unlikePost(this.post.PostId).subscribe(
      () => {
        this.post.IsLikedByCurrentUser = false;
        this.post.LikeCount--;
      },
      (error) => {
        console.error('Error unliking post:', error);
      }
    );
  }

  toggleComments(): void {
    this.commentsVisible = !this.commentsVisible;
  }

  onCommentAdded(newComment: CommentDTO): void {
    this.post.CommentCount++;
    if (this.commentsVisible && this.commentList) {
      this.commentList.loadPostComments();
   }
  }

  isImage(contentType: string | undefined): boolean {
    if (!contentType) {
      return false;
    }
    return contentType.startsWith('image/');
  }

}