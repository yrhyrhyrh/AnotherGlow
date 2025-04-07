import { Component, EventEmitter, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';
import { GroupRequest } from '../../interfaces/groupRequest';

@Component({
  selector: 'app-create-new-group',
  imports: [FormsModule],
  standalone: true,
  templateUrl: './create-new-group.component.html',
  styleUrl: './create-new-group.component.css'
})

export class CreateNewGroupComponent {
  groupRequest: GroupRequest = {
    Name: "",
    UserId: "",
  }

  constructor(private http: HttpClient, private router: Router) {
    const token = localStorage.getItem('jwt_token');
    if (token) {
      try {
        const decoded: any = jwtDecode(token);
        this.groupRequest.UserId = decoded.userId || ""; // Set userId if available
      } catch (error) {
        console.error("Invalid token:", error);
      }
    } else {
      console.warn("No JWT token found in localStorage");
    }
  }
  
  @Output() groupCreated = new EventEmitter<string>();
  @Output() close = new EventEmitter<void>();

  createGroup() {
    if (this.groupRequest.Name.trim()) {
      console.log(this.groupRequest);
      this.http.post<{ token: string }>('http://localhost:5181/api/group/create', this.groupRequest)
      .subscribe({
        next: (response) => {

          this.groupCreated.emit(this.groupRequest.Name);
          console.log(response);
        },
        error: (error) => {
          console.error("Request error:", error);
        }
      });
    }
  }

  closeDialog() {
    this.close.emit();
  }
}
