import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, Subject } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';

// Interfaces
export interface Poll {
  PollId?: string;
  Question: string;
  Options: string[];
  UserId?: string;
  AllowMultipleSelections: boolean;
  Votes?: { [key: number]: number };
}

interface VotePayload {
  pollId: string;
  userId: string;
  optionIndex?: number;
  optionIndices?: number[];
  retract: boolean;
}

interface VoteResponse {
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class PollService {
  private apiUrl = `${environment.apiUrl}/api/polls`;

  //   Observer Pattern
  private pollChanged = new Subject<void>();
  pollChanged$ = this.pollChanged.asObservable();

  constructor(private http: HttpClient) { }

  private getAuthHeaders(): HttpHeaders {
    let headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    const token = localStorage.getItem('jwt_token');
    if (token) {
      headers = headers.append('Authorization', `Bearer ${token}`);
    }
    return headers;
  }

  //   Emit event after creation
  createPoll(pollData: any): Observable<VoteResponse> {
    const headers = this.getAuthHeaders();
    return this.http.post<VoteResponse>(`${this.apiUrl}/create`, pollData, { headers }).pipe(
      tap(() => this.pollChanged.next()), // 🔁 Notify subscribers
      catchError(this.handleError)
    );
  }

  getAllPolls(): Observable<Poll[]> {
    const headers = this.getAuthHeaders();
    return this.http.get<Poll[]>(`${this.apiUrl}/all`, { headers }).pipe(
      catchError(this.handleError)
    );
  }

  getPollById(pollId: string): Observable<Poll> {
    return this.http.get<Poll>(`${this.apiUrl}/${pollId}`);
  }

  //   Emit after vote
  castVote(voteData: VotePayload): Observable<VoteResponse> {
    const headers = this.getAuthHeaders();
    const url = `${this.apiUrl}/${voteData.pollId}/vote`;
    return this.http.post<VoteResponse>(url, voteData, { headers }).pipe(
      tap(() => this.pollChanged.next()),
      catchError(this.handleError)
    );
  }

  //   Emit after retraction
  retractVote(pollId: string, userId: string): Observable<VoteResponse> {
    const headers = this.getAuthHeaders();
    const url = `${this.apiUrl}/${pollId}/vote`;
    const body = { userId, retract: true };
    return this.http.post<VoteResponse>(url, body, { headers }).pipe(
      tap(() => this.pollChanged.next()),
      catchError(this.handleError)
    );
  }

  private handleError(error: HttpErrorResponse) {
    let errorMessage = 'An unknown error occurred!';
    if (error.error instanceof ErrorEvent) {
      errorMessage = `Error: ${error.error.message}`;
    } else {
      errorMessage = `Server returned code ${error.status}, error message is: ${error.message}`;
    }
    return throwError(() => new Error(errorMessage));
  }
}
