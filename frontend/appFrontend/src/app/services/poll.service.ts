import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

// Define a Poll interface to represent the structure of a poll object
export interface Poll {
  id: number;
  question: string;
  options: string[];
  isGlobal: boolean;
  // Add other properties as needed
}

@Injectable({
  providedIn: 'root'
})
export class PollService {
  private apiUrl = 'http://localhost:5181/api/polls'; // Base URL (adjust port if different)

  constructor(private http: HttpClient) { }

  createPoll(pollData: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/create`, pollData) // Specify the type
      .pipe(
        catchError(this.handleError)  // Use the new handleError method
      );
  }

  getAllPolls(): Observable<Poll[]> {  // Specify the return type
    return this.http.get<Poll[]>(`${this.apiUrl}/all`) // Specify the type and use back ticks
      .pipe(
        catchError(this.handleError) // Use the new handleError method
      );
  }

  // Generic error handling method
  private handleError(error: HttpErrorResponse) {
    if (error.error instanceof ErrorEvent) {
      // A client-side or network error occurred. Handle it accordingly.
      console.error('An error occurred:', error.error.message);
    } else {
      // The backend returned an unsuccessful response code.
      // The response body may contain clues as to what went wrong.
      console.error(
        `Backend returned code ${error.status}, ` +
        `body was: ${error.error}`);
    }
    // Return an observable with a user-facing error message.
    return throwError(() => 'Something bad happened; please try again later.');
  }
    castVote(pollId: number, optionIndex: number): Observable<any> {
    const url = `${this.apiUrl}/${pollId}/vote`;  // Construct the URL
    return this.http.post(url, optionIndex)  // Send the optionIndex in the body
      .pipe(catchError(this.handleError));  // Handle errors
  }
}