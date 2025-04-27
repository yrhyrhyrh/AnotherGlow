import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
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
import { RouterModule, Router } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ConfirmationDialogComponent } from '../../shared/confirmation-dialog/confirmation-dialog.component';
import { GroupService } from '../../services/group.service';

interface GroupData {
  GroupId: string;
  Name: string;
  Description: string;
  GroupPictureUrl?: string;
  IsAdmin: boolean;
  Members: Array<{
    GroupMemberId: string;
    IsAdmin: boolean;
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
    RouterModule,
    MatDialogModule
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
  error: string | null = null;
  bannerStyle: any = {};

  constructor(
    private route: ActivatedRoute,
    private groupService: GroupService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private router: Router
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
    this.groupService.getGroupDetails(this.groupId!).subscribe({
      next: (data) => {
        console.log('Group data received:', data);
        console.log('Group picture URL:', data.GroupPictureUrl);
        this.groupData = data;
        this.isLoading = false;

        // Set banner background image
        if (data.GroupPictureUrl) {
          this.bannerStyle = {
            'background-image': `url(${data.GroupPictureUrl})`
          };
        } else {
          console.log('No group picture URL, using default gradient');
          // Use a default gradient background if no image
          this.bannerStyle = {
            'background-image': 'linear-gradient(135deg, #6a11cb 0%, #2575fc 100%)'
          };
        }
      },
      error: (err) => {
        console.error('Error fetching group details:', err);
        this.isLoading = false;
        this.error = 'Failed to load group details';
        this.snackBar.open('Failed to load group details', 'Close', {
          duration: 3000
        });
      }
    });
  }

  fetchUsersNotInGroup(keyword: string) {
    this.groupService.searchUsersNotInGroup(this.groupId!, keyword).subscribe({
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

    this.groupService.addMembers(request).subscribe({
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

  editGroup() {
    this.router.navigate(['/group', this.groupId, 'edit']);
  }

  makeAdmin(member: any) {
    this.groupService.makeAdmin(member.GroupMemberId).subscribe({
      next: () => {
        this.snackBar.open('Member made admin successfully', 'Close', {
          duration: 3000
        });
        this.fetchGroupDetails();
      },
      error: (error) => {
        console.error('Error making member admin:', error);
        this.snackBar.open('Failed to make member admin', 'Close', {
          duration: 3000
        });
      }
    });
  }

  removeMember(member: any) {
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      width: '300px',
      data: {
        title: 'Remove Member',
        message: 'Are you sure you want to remove this member from the group?'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.groupService.removeMember(member.GroupMemberId).subscribe({
          next: () => {
            this.snackBar.open('Member removed successfully', 'Close', {
              duration: 3000
            });
            this.fetchGroupDetails();
          },
          error: (error) => {
            console.error('Error removing member:', error);
            this.snackBar.open('Failed to remove member', 'Close', {
              duration: 3000
            });
          }
        });
      }
    });
  }
}
