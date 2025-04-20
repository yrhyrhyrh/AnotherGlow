import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MemberComponent } from './member/member.component';
import { SearchBarComponent } from '../../shared/search-bar/search-bar.component';
import { UserSuggestion } from '../../interfaces/userSuggestion';
import { AddNewMemberRequest } from '../../interfaces/addNewMemberRequest';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterModule } from '@angular/router';
import { environment } from '../../../environments/environment';

interface GroupData {
  GroupId: string;
  Name: string;
  Description: string;
  Members: Array<{
    User: {
      UserId: string;
      Username: string;
      ProfilePictureUrl: string;
    };
  }>;
}

@Component({
  selector: 'app-group-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MemberComponent,
    SearchBarComponent,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatSnackBarModule,
    MatToolbarModule,
    MatListModule,
    MatBadgeModule,
    MatTooltipModule,
    RouterModule
  ],
  templateUrl: './groupdetail.component.html',
  styleUrl: './groupdetail.component.css'
})
export class GroupdetailComponent implements OnInit {
  groupId: string | null = null;
  groupData: GroupData | null = null;
  userSuggestions: UserSuggestion[] = [];
  usersToAdd: Set<UserSuggestion> = new Set<UserSuggestion>();
  isLoading = true;

  constructor(
    private route: ActivatedRoute,
    private http: HttpClient,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      this.groupId = params.get('id');
      if (this.groupId) {
        this.fetchGroupDetails();
      }
    });
  }

  fetchGroupDetails() {
    this.isLoading = true;
    this.http.get<GroupData>(`${environment.apiUrl}/api/group/detail/${this.groupId}`)
      .subscribe({
        next: (data) => {
          this.groupData = data;
          this.isLoading = false;
        },
        error: (err) => {
          console.error('Error fetching group details:', err);
          this.isLoading = false;
          this.snackBar.open('Failed to load group details', 'Close', {
            duration: 3000
          });
        }
      });
  }

  fetchUsersNotInGroup(keyword: string) {
    this.http.get<UserSuggestion[]>(`${environment.apiUrl}/api/group/getUsersToAdd/${this.groupId}?keyword=${keyword}`)
      .subscribe({
        next: (data) => {
          this.userSuggestions = data;
        },
        error: (err) => {
          console.error("Failed to fetch users:", err);
          this.userSuggestions = [];
          this.snackBar.open('Failed to fetch users', 'Close', {
            duration: 3000
          });
        }
      });
  }

  addUsersToGroup() {
    if (this.usersToAdd.size === 0) {
      this.snackBar.open('Please select users to add', 'Close', {
        duration: 3000
      });
      return;
    }

    const request: AddNewMemberRequest = {
      GroupId: this.groupId!,
      UserIds: [...this.usersToAdd].map((user) => user.UserId),
    };

    this.http.post<{ token: string }>(`${environment.apiUrl}/api/group/addMembers`, request)
      .subscribe({
        next: (response) => {
          this.snackBar.open('Users added successfully', 'Close', {
            duration: 3000
          });
          this.usersToAdd.clear();
          this.fetchGroupDetails();
        },
        error: (error) => {
          console.error("Request error:", error);
          this.snackBar.open('Failed to add users', 'Close', {
            duration: 3000
          });
        }
      });
  }

  onSearch(keyword: string) {
    this.fetchUsersNotInGroup(keyword);
  }

  handleSelect(user: UserSuggestion) {
    this.usersToAdd.add(user);
  }

  removeUser(user: UserSuggestion) {
    this.usersToAdd.delete(user);
  }
}
