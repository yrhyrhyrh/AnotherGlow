import { Component, OnInit } from '@angular/core';
import { PostService } from '../../services/post.service';
import { PostDTO } from '../../models/postDto';
import { CommonModule } from '@angular/common';          // Import CommonModule
import { MatListModule } from '@angular/material/list'; // Import MatListModule
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatToolbar, MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { PostCreateComponent } from '../post-create/post-create.component';
import { PostCardComponent } from '../post-card/post-card.component';
import { ActivatedRoute, RouterModule } from '@angular/router'; // Import ActivatedRoute and RouterModule
import { HttpResponse } from '@angular/common/http'; // Import HttpResponse

@Component({
  selector: 'app-post-list',
  standalone: true,
  imports: [
    MatListModule,
    MatPaginatorModule,
    CommonModule,
    MatToolbar,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    RouterModule,
    PostCreateComponent,
    PostCardComponent
  ],
  templateUrl: './post-list.component.html',
  styleUrls: ['./post-list.component.css']
})
export class PostListComponent implements OnInit {
  posts: PostDTO[] = [];
  currentPage = 1;
  pageSize = 5;
  totalPosts = 0;
  totalPages = 0;
  pageNumbers: number[] = [];
  groupId: string | null = null;

  constructor(
    private postService: PostService,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => { // Subscribe to paramMap
      this.groupId = params.get('groupId');
      if (this.groupId == "general") {
        //TODO: retrieve from groupservice after ryan create
        this.groupId = '5cfb7a3f-b15d-4089-aae8-d9dafc2202c6';
      }
      this.loadGlobalPosts();
    });
  }

  loadGlobalPosts(): void {
    console.log("fetching posts for " + `${this.groupId}`)
    if (!this.groupId) {
      console.error('Group ID is missing.');
      return; // Don't load posts if groupId is missing
    }
    this.postService.getGlobalPosts(this.currentPage, this.pageSize, this.groupId).subscribe({ // Pass groupId to service
      next: (response) => {
        console.log(response.body);
        this.posts = response.body || [];
        this.totalPosts = Number(response.headers.get('X-Total-Count') || '0');
        this.totalPages = Math.ceil(this.totalPosts / this.pageSize);
        this.generatePageNumbers();
      },
      error: (error) => {
        console.error('Error loading posts:', error);
        this.posts = [];
        this.totalPosts = 0;
        this.totalPages = 0;
        this.pageNumbers = [];
      }
    });
  }

  handlePageEvent(event: PageEvent): void {
    this.currentPage = event.pageIndex + 1; // Convert 0-based to 1-based
    this.pageSize = event.pageSize;
    this.loadGlobalPosts();
  }

  generatePageNumbers(): void {
    console.log("total posts", this.totalPosts)
    this.pageNumbers = [];
    for (let i = 1; i <= this.totalPages; i++) {
      this.pageNumbers.push(i);
    }
  }

  goToPage(pageNumber: number): void {
    if (pageNumber >= 1 && pageNumber <= this.totalPages) {
      this.currentPage = pageNumber;
      this.loadGlobalPosts();
    }
  }

  previousPage(): void {
    this.goToPage(this.currentPage - 1);
  }

  nextPage(): void {
    this.goToPage(this.currentPage + 1);
  }
}