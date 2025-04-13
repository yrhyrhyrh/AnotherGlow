import { Component, Input, OnInit } from '@angular/core';
import { SocialActionsService } from '../../services/social-actions.service';
import { CommentDTO } from '../../models/postDto';
import { MatListModule } from '@angular/material/list'; // Import MatListModule
import { CommonModule } from '@angular/common';      // Import CommonModule

@Component({
  selector: 'app-comment-list',
  standalone: true,
  imports: [
    MatListModule, // Add MatListModule to imports
    CommonModule   // Add CommonModule to imports
  ],
  templateUrl: './comment-list.component.html',
  styleUrl: './comment-list.component.css'
})
export class CommentListComponent implements OnInit {
  @Input() PostId!: string;
  comments: CommentDTO[] = [];

  constructor(private socialActionsService: SocialActionsService) { }

  ngOnInit(): void {
    this.loadPostComments();
  }

  loadPostComments(): void {
    this.socialActionsService.getCommentsForPost(this.PostId).subscribe(
      (comments) => {
        this.comments = comments;
      },
      (error) => {
        console.error('Error loading comments:', error);
      }
    );
  }
}