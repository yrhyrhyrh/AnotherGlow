import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpResponse  } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PostDTO, CreatePostRequestDTO, UpdatePostRequestDTO } from '../models/postDto'; // Import DTOs (create these in next step if not already done)
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class PostService {
  private baseUrl = environment.apiUrl;
  private apiUrl = `${this.baseUrl}/api/posts`;

  constructor(private http: HttpClient) { }

  getGlobalPosts(pageNumber: number, pageSize: number, groupId: string | null): Observable<HttpResponse<PostDTO[]>> { // Accept groupId
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString())
      .set('_t', Date.now().toString()); // Cache buster

    if (groupId) { // Add groupId parameter if it exists
      params = params.set('groupId', groupId);
    }

    return this.http.get<PostDTO[]>(this.apiUrl, { params: params, observe: 'response' })
      .pipe(
        // ... pipe operators ...
      );
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