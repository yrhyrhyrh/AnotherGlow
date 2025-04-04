import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-member',
  imports: [],
  standalone: true,
  templateUrl: './member.component.html',
  styleUrl: './member.component.css'
})
export class MemberComponent {
  @Input() member!: { username: string; profilePictureUrl: string; userId: string };
  

}
