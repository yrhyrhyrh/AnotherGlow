import { Component, OnInit } from '@angular/core';
import { PostService } from '../../services/post.service';
import { PostDTO } from '../../models/dto';
import { CommonModule } from '@angular/common';          // Import CommonModule
import { MatListModule } from '@angular/material/list'; // Import MatListModule
import { MatToolbar, MatToolbarModule } from '@angular/material/toolbar';
import { PostCreateComponent } from '../post-create/post-create.component';
import { PostCardComponent } from '../post-card/post-card.component';

@Component({
  selector: 'app-post-list',
  standalone: true,
  imports: [
    MatListModule,
    CommonModule,
    MatToolbar,
    PostCreateComponent,
    PostCardComponent
  ],
  templateUrl: './post-list.component.html',
  styleUrls: ['./post-list.component.css']
})
export class PostListComponent implements OnInit {
  posts: PostDTO[] = [];

  constructor(private postService: PostService) { }

  ngOnInit(): void {
    this.loadGlobalPosts();
  }

  loadGlobalPosts(): void {
    this.postService.getGlobalPosts().subscribe(
      (posts) => {
        console.log(posts)
        this.posts = posts;
      },
      (error) => {
        console.error('Error loading posts:', error);
      }
    );
  }
}