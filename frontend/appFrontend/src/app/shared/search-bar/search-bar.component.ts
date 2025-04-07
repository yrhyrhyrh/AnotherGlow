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
  @Input() suggestions: UserSuggestion[] = [];
  @Input() maxSuggestions: number = 5;

  @Output() search = new EventEmitter<string>();
  @Output() suggestionClicked = new EventEmitter<string>();

  searchTerm: string = '';

  onSearch() {
    this.search.emit(this.searchTerm.trim());
  }

  onSelectSuggestion(suggestion: string) {
    this.suggestionClicked.emit(suggestion);
    this.search.emit(suggestion);
  }

  get filteredSuggestions() {
    return this.suggestions
      .filter(s => s.Username.toLowerCase().includes(this.searchTerm.toLowerCase()))
      .slice(0, this.maxSuggestions);
  }
}
