import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserSuggestion } from '../../interfaces/userSuggestion';

@Component({
  selector: 'app-search-bar',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './search-bar.component.html',
  styleUrl: './search-bar.component.css'
})
export class SearchBarComponent {
  @Input() placeholder: string = 'Search...';
  @Input() suggestions: any[] = [];
  @Input() maxSuggestions: number = 5;

  @Output() search = new EventEmitter<string>();
  @Output() suggestionClicked = new EventEmitter<any>();

  searchTerm: string = '';

  onSearch() {
    this.search.emit(this.searchTerm.trim());
  }

  onSelectSuggestion(suggestion: any) {
    this.suggestionClicked.emit(suggestion);
  }
}
