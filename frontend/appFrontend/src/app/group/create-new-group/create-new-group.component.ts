import { Component, EventEmitter, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Group } from '../../interfaces/group';

@Component({
  selector: 'app-create-new-group',
  imports: [FormsModule],
  standalone: true,
  templateUrl: './create-new-group.component.html',
  styleUrl: './create-new-group.component.css'
})
export class CreateNewGroupComponent {
  groupRequest: Group = {
    groupName: "",
    userId: "050b4ef3-acdb-4352-86f1-195e61b0147d",
  }
  constructor(private http: HttpClient, private router: Router) {}
  
  @Output() groupCreated = new EventEmitter<string>();
  @Output() close = new EventEmitter<void>();

  createGroup() {
    if (this.groupRequest.groupName.trim()) {
      console.log(this.groupRequest);
      this.http.post<{ token: string }>('http://localhost:5181/api/group/create', this.groupRequest)
      .subscribe({
        next: (response) => {

        this.groupCreated.emit(this.groupRequest.groupName);
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
