import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';

// Consistent Poll Interface (matches backend casing if backend sends PascalCase)
export interface Poll {
  PollId?: string;
  Question: string;
  Options: string[];
  IsGlobal: boolean;
  UserId?: string;
  AllowMultipleSelections: boolean;
  Votes?: { [key: number]: number }; // Assuming Votes dictionary is also sent
}

// Interface for the vote request body
interface VotePayload {
  pollId: string;
  userId: string;
  optionIndex?: number;      // Nullable for multi-select
  optionIndices?: number[]; // Nullable for single-select
  retract: boolean;
}

// Interface for the vote response body
interface VoteResponse {
  message: string;
}


@Injectable({
  providedIn: 'root'
})
export class PollService {
  // Use HTTPS and correct port based on your launchSettings.json
  private apiUrl = `${environment.apiUrl}/api/polls`; // <--- ADJUST PORT IF NEEDED

  constructor(private http: HttpClient) { }

  // Centralized method to get headers (includes JWT)
  private getAuthHeaders(): HttpHeaders {
    let headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    const token = localStorage.getItem('jwt_token');
    console.log("Retrieved token:", token); // Log token
    if (token) {
      const authHeaderValue = `Bearer ${token}`;
      console.log("Setting Authorization header:", authHeaderValue); // Log header value
      headers = headers.append('Authorization', authHeaderValue);
    } else {
      console.warn("No JWT token found in localStorage."); // Warn if missing
    }
    return headers;
  }

  // Create a new poll
  createPoll(pollData: any): Observable<VoteResponse> { // Expect VoteResponse
    const headers = this.getAuthHeaders();
    console.log('PollService - createPoll sending:', pollData);
    return this.http.post<VoteResponse>(`${this.apiUrl}/create`, pollData, { headers })
      .pipe(catchError(this.handleError));
  }

  // Get all polls
  getAllPolls(): Observable<Poll[]> {
    const headers = this.getAuthHeaders();
    console.log('PollService - getAllPolls');
    return this.http.get<Poll[]>(`${this.apiUrl}/all`, { headers })
      .pipe(catchError(this.handleError));
  }

  getPollById(pollId: string): Observable<Poll> {
    return this.http.get<Poll>(`${this.apiUrl}/${pollId}`);
  }  

  // Cast a vote (handles single or multiple based on payload)
  castVote(voteData: VotePayload): Observable<VoteResponse> {
    const headers = this.getAuthHeaders();
    const url = `${this.apiUrl}/${voteData.pollId}/vote`;
    console.log(`PollService - castVote sending to ${url}:`, voteData);
    // Send the structured voteData object as the body
    return this.http.post<VoteResponse>(url, voteData, { headers })
      .pipe(catchError(this.handleError));
  }

   retractVote(pollId: string, userId: string): Observable<VoteResponse> {
     const headers = this.getAuthHeaders();
     const url = `${this.apiUrl}/${pollId}/vote`;
     const body = { userId, retract: true };
     return this.http.post<VoteResponse>(url, body, { headers })
       .pipe(catchError(this.handleError));
   }

  // Centralized Error Handler
  private handleError(error: HttpErrorResponse) {
    let errorMessage = 'An unknown error occurred!';
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Error: ${error.error.message}`;
      console.error('Client-side error:', error.error.message);
    } else {
      // Server-side error
      errorMessage = `Server returned code ${error.status}, error message is: ${error.message}`;
      console.error(
        `Backend returned code ${error.status} (${error.statusText}), ` +
        `body was:`, error.error // Log the actual error body if available
      );
      // Potentially extract more specific error message from error.error
      if (typeof error.error === 'string') {
        errorMessage = error.error;
      } else if (error.error && typeof error.error.message === 'string') {
        errorMessage = error.error.message;
      } else if (typeof error.message === 'string') {
        errorMessage = error.message;
      }
    }
    // Return an observable with a user-facing error message.
    return throwError(() => new Error(errorMessage));
  }
}
