import { Component } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { HttpClient } from '@angular/common/http';
import { RouterModule, Router } from '@angular/router';
import { CreateNewGroupComponent } from './create-new-group/create-new-group.component';
import { CommonModule } from "@angular/common";
import { GroupCardComponent } from "./group-card/group-card.component";
import { jwtDecode } from "jwt-decode";
import { GetGroups } from '../interfaces/getGroups';

@Component({
  selector: "app-group",
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, CreateNewGroupComponent, GroupCardComponent],
  templateUrl: "./group.component.html",
  styleUrls: ["./group.component.css"]
})
export class GroupComponent {
  showDialog = false;
  adminGroups = [
    { name: "Admin Group 1", description: "This is an admin group", groupId: "1" },
    { name: "Admin Group 2", description: "Another admin group", groupId: "2" }
  ];

  nonAdminGroups = [
    { name: "Non-Admin Group 1", description: "Just a regular group", groupId: "3" },
    { name: "Non-Admin Group 2", description: "Another regular group", groupId: "4" }
  ];
  userId="";

  constructor(private http: HttpClient, private router: Router) {
    const token = localStorage.getItem('jwt_token');
    if (token) {
      try {
        const decoded: any = jwtDecode(token);
        this.userId = decoded.userId || ""; // Set userId if available
        const adminGroupsRequest: GetGroups = {
          userId: this.userId,
          isAdmin: true,
        }
        const nonAdminGroupsRequest: GetGroups = {
          userId: this.userId,
          isAdmin: false,
        }

        this.http.post<{ groups: any[] }>('http://localhost:5181/api/group/getbyuserid', adminGroupsRequest)
        .subscribe({
          next: (response) => {

            this.adminGroups = response.groups
            console.log(response);
          },
          error: (error) => {
            console.error("Request error:", error);
          }
        });

        this.http.post<{ groups: any[] }>('http://localhost:5181/api/group/getbyuserid', nonAdminGroupsRequest)
        .subscribe({
          next: (response) => {

            this.nonAdminGroups = response.groups
            console.log(response);
          },
          error: (error) => {
            console.error("Request error:", error);
          }
        });
      } catch (error) {
        console.error("Invalid token:", error);
      }
    } else {
      console.warn("No JWT token found in localStorage");
    }


  }

  navigateToGroup(groupId: string) {
    console.log("Navigating to group:", groupId);
    // Implement your navigation logic here
  }

  openDialog() {
    this.showDialog = true;
  }

  closeDialog() {
    this.showDialog = false;
  }

  handleGroupCreation(groupName: string) {
    console.log("New group created:", groupName);
    this.closeDialog();
  }
}
