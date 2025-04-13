import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LikeDTO, CommentDTO, CreateCommentRequestDTO } from '../models/dto'; // Import DTOs
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SocialActionsService {
  private baseUrl = environment.apiUrl;
  private apiUrl = `${this.baseUrl}/api/social-actions`; // Adjust if your API endpoint is different

  constructor(private http: HttpClient) { }

  likePost(PostId: string): Observable<LikeDTO> {
    return this.http.post<LikeDTO>(`${this.apiUrl}/posts/${PostId}/like`, {});
  }

  unlikePost(PostId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/posts/${PostId}/unlike`, {});
  }

  addCommentToPost(PostId: string, commentData: CreateCommentRequestDTO): Observable<CommentDTO> {
    return this.http.post<CommentDTO>(`${this.apiUrl}/posts/${PostId}/comments`, commentData);
  }

  deleteComment(CommentId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/comments/${CommentId}`);
  }

  getCommentsForPost(PostId: string): Observable<CommentDTO[]> {
      return this.http.get<CommentDTO[]>(`${this.apiUrl}/posts/${PostId}/comments`);
    }
}