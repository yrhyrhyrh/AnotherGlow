import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { UserSuggestion } from '../../interfaces/userSuggestion';

@Component({
  selector: 'app-search-bar',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatListModule,
    MatProgressBarModule
  ],
  templateUrl: './search-bar.component.html',
  styleUrl: './search-bar.component.css'
})
export class SearchBarComponent {
  @Input() placeholder: string = 'Search...';
  @Input() suggestions: UserSuggestion[] = [];
  @Input() maxSuggestions: number = 5;
  @Input() isLoading: boolean = false;

  @Output() search = new EventEmitter<string>();
  @Output() suggestionClicked = new EventEmitter<UserSuggestion>();

  searchTerm: string = '';

  onSearch() {
    this.search.emit(this.searchTerm.trim());
  }

  onSelectSuggestion(suggestion: UserSuggestion) {
    this.suggestionClicked.emit(suggestion);
    this.searchTerm = '';
  }
}
