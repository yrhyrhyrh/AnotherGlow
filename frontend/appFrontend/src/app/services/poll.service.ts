import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

export interface Poll {
  PollId: string; // Matches backend property
  Question: string;
  Options: string[];
  IsGlobal: boolean;
  UserId: string; // Add this to match the component’s interface
}

@Injectable({
  providedIn: 'root'
})
export class PollService {
  private apiUrl = 'http://localhost:5181/api/polls';

  constructor(private http: HttpClient) { }

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('jwt_token');
    if (token) {
      return new HttpHeaders({ 'Authorization': `Bearer ${token}` });
    }
    return new HttpHeaders();
  }

  createPoll(pollData: any): Observable<any> {
    const headers = this.getAuthHeaders();
    return this.http.post(`${this.apiUrl}/create`, pollData, { headers })
      .pipe(catchError(this.handleError));
  }

  getAllPolls(): Observable<Poll[]> {
    const headers = this.getAuthHeaders();
    return this.http.get<Poll[]>(`${this.apiUrl}/all`, { headers })
      .pipe(catchError(this.handleError));
  }

  castVote(pollId: string, optionIndex: number, userId: string): Observable<{ message: string }> {
    console.log('PollService - castVote');
    console.log('pollId:', pollId);
    console.log('optionIndex:', optionIndex);
    console.log('userId:', userId);

    const voteRequest = {
      userId: userId,
      optionIndex: optionIndex,
      retract: false
    };
    console.log('voteRequest:', voteRequest);
    const headers = this.getAuthHeaders();
    return this.http.post<{ message: string }>(`${this.apiUrl}/${pollId}/vote`, voteRequest);
  }

  private handleError(error: HttpErrorResponse) {
    if (error.error instanceof ErrorEvent) {
      console.error('Client-side error:', error.error.message);
    } else {
      console.error(`Server returned code ${error.status}, body was:`, error.error);
    }
    return throwError(() => new Error('Something went wrong. Please try again later.'));
  }
}
