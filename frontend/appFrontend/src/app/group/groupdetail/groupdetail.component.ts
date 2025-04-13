import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { MemberComponent } from './member/member.component';
import { SearchBarComponent } from '../../shared/search-bar/search-bar.component';
import { UserSuggestion } from '../../interfaces/userSuggestion';
import { AddNewMemberRequest } from '../../interfaces/addNewMemberRequest';

@Component({
  selector: 'app-group-detail',
  standalone: true,
  imports: [CommonModule, MemberComponent, SearchBarComponent],
  templateUrl: './groupdetail.component.html',
  styleUrl: './groupdetail.component.css'
})
export class GroupdetailComponent implements OnInit {
  groupId: string | null = null;
  groupData: any = null;
  userSuggestions: UserSuggestion[] = [];
  usersToAdd: Set<UserSuggestion> = new Set<UserSuggestion>();
  constructor(private route: ActivatedRoute, private http: HttpClient) {}

  ngOnInit() {
    // Extract group ID from the route parameters
    this.route.paramMap.subscribe(params => {
      this.groupId = params.get('id');  // Get ID from the URL

      if (this.groupId) {
        this.fetchGroupDetails();
      }
    });
  }

  fetchGroupDetails() {
    this.http.get(`http://localhost:5181/api/group/detail/${this.groupId}`)
      .subscribe({
        next: (data) => {
          console.log("data is:",data);
          this.groupData = data;
        },
        error: (err) => console.error('Error fetching group details:', err)
      });
  }

  fetchUsersNotInGroup(keyword: string) {
    this.http.get<UserSuggestion[]>(`http://localhost:5181/api/group/getUsersToAdd/${this.groupId}?keyword=${keyword}`)
      .subscribe({
        next: (data) => {
          console.log("users:", data);
          this.userSuggestions = data;
        },
        error: (err) => {
          console.error("Failed to fetch users:", err);
          this.userSuggestions = [];
        }
      });
  }

  addUsersToGroup() {
    var request: AddNewMemberRequest = {
      GroupId: this.groupId!,
      UserIds: [...this.usersToAdd].map((user)=> user.UserId),
    }
    this.http.post<{ token: string }>('http://localhost:5181/api/group/addMembers', request)
      .subscribe({
        next: (response) => {
          location.reload();
        },
        error: (error) => {
          console.error("Request error:", error);
        }
      });
  }

  onSearch(keyword: string) {
    // Call your service or filter logic here
    this.fetchUsersNotInGroup(keyword);
  };

  handleSelect(user: UserSuggestion) {
    console.log("Adding id:",user.UserId);
    this.usersToAdd.add(user);
  }
}
