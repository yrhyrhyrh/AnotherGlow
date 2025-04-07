import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-member',
  imports: [],
  standalone: true,
  templateUrl: './member.component.html',
  styleUrl: './member.component.css'
})
export class MemberComponent {
  @Input() member!: { Username: string; ProfilePictureUrl: string; };

  ngOnInit(): void {
    console.log("Member received:", this.member);
  }
}
