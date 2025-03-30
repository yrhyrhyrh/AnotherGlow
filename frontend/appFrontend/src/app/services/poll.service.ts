import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class PollService {
  private apiUrl = 'https://localhost:5181/api/polls/create'; // Check this carefully!

  constructor(private http: HttpClient) { }

  createPoll(pollData: any): Observable<any> {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    };

    console.log('Sending request to:', this.apiUrl);  // Log the URL
    console.log('Request data:', pollData);  // Log the data

    return this.http.post(this.apiUrl, pollData, httpOptions)
      .pipe(
        catchError(error => {
          console.error('HTTP Error:', error);  // Log the full error
          return throwError(() => error);  // Re-throw the error
        })
      );
  }
}
