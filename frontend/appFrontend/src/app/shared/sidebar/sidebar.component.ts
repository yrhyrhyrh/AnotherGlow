import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { HttpClient } from '@angular/common/http';
import { jwtDecode } from "jwt-decode";
import { GetGroups } from '../../interfaces/getGroups';
import { Group } from "../../interfaces/group";
import { CreateGroupButtonComponent } from '../../group/create-group-button/create-group-button.component';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    CreateGroupButtonComponent
  ],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent implements OnInit {
  adminGroups: Group[] = [];
  nonAdminGroups: Group[] = [];
  userId = "";

  constructor(private http: HttpClient) {
    const token = localStorage.getItem('jwt_token');
    if (token) {
      try {
        const decoded: any = jwtDecode(token);
        this.userId = decoded.userId || "";
        this.loadGroups();
      } catch (error) {
        console.error("Invalid token:", error);
      }
    }
  }

  ngOnInit(): void {
    // Additional initialization if needed
  }

  loadGroups() {
    const adminGroupsRequest: GetGroups = {
      userId: this.userId,
      isAdmin: true,
    }
    const nonAdminGroupsRequest: GetGroups = {
      userId: this.userId,
      isAdmin: false,
    }

    this.http.post<{ groups: Group[] }>('http://localhost:5181/api/group/getbyuserid', adminGroupsRequest)
      .subscribe({
        next: (response) => {
          this.adminGroups = response.groups;
        },
        error: (error) => {
          console.error("Request error:", error);
        }
      });

    this.http.post<{ groups: Group[] }>('http://localhost:5181/api/group/getbyuserid', nonAdminGroupsRequest)
      .subscribe({
        next: (response) => {
          this.nonAdminGroups = response.groups;
        },
        error: (error) => {
          console.error("Request error:", error);
        }
      });
  }
} 