import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PostDTO, CreatePostRequestDTO, UpdatePostRequestDTO } from '../models/dto'; // Import DTOs (create these in next step if not already done)
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class PostService {
  private baseUrl = environment.apiUrl;
  private apiUrl = `${this.baseUrl}/api/posts`;

  constructor(private http: HttpClient) { }

  getGlobalPosts(): Observable<PostDTO[]> {
    return this.http.get<PostDTO[]>(this.apiUrl);
  }

  getPostById(PostId: string): Observable<PostDTO> {
    return this.http.get<PostDTO>(`${this.apiUrl}/${PostId}`);
  }

  createPost(postData: CreatePostRequestDTO): Observable<PostDTO> {
    console.log("postdata:", postData);
    return this.http.post<PostDTO>(this.apiUrl, postData);
  }

  updatePost(PostId: string, postData: UpdatePostRequestDTO): Observable<PostDTO> {
    return this.http.put<PostDTO>(`${this.apiUrl}/${PostId}`, postData);
  }

  deletePost(PostId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${PostId}`);
  }
}