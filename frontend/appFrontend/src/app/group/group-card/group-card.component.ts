import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-group-card',
  standalone: true,
  imports: [],
  templateUrl: './group-card.component.html',
  styleUrls: ['./group-card.component.css']
})
export class GroupCardComponent {
  @Input() group!: { Name: string; GroupId: string };
  @Output() clicked = new EventEmitter<string>();

  onClick() {
    this.clicked.emit(this.group.GroupId);
  }
}
