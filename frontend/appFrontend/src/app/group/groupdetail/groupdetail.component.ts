import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { MemberComponent } from './member/member.component';

@Component({
  selector: 'app-group-detail',
  standalone: true,
  imports: [CommonModule, MemberComponent],
  templateUrl: './groupdetail.component.html',
  styleUrl: './groupdetail.component.css'
})
export class GroupdetailComponent implements OnInit {
  groupId: string | null = null;
  groupData: any = null;

  constructor(private route: ActivatedRoute, private http: HttpClient) {}

  ngOnInit() {
    // Extract group ID from the route parameters
    this.route.paramMap.subscribe(params => {
      this.groupId = params.get('id');  // Get ID from the URL

      if (this.groupId) {
        this.fetchGroupDetails(this.groupId);
      }
    });
  }

  fetchGroupDetails(groupId: string) {
    this.http.get(`http://localhost:5181/api/group/detail/${groupId}`)
      .subscribe({
        next: (data) => {
          console.log("data is:",data);
          this.groupData = data;
        },
        error: (err) => console.error('Error fetching group details:', err)
      });
  }
}
