import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PollService {
  private apiUrl = 'http://localhost:5181/api/polls'; // Base URL (adjust port if different)

  constructor(private http: HttpClient) { }

  createPoll(pollData: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/create`, pollData);  // append /create on to the end.
  }

  getAllPolls(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/all`); // Get all polls (adjust URL if different)
  }
}