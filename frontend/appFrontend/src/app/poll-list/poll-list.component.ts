import { Component, OnInit } from '@angular/core';
import { PollService, Poll } from '../services/poll.service'; // Import Poll interface, verify path
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../services/auth.service'; // Verify path

@Component({
  selector: 'app-poll-list',
  standalone: true,
  imports: [CommonModule, FormsModule], // Keep dependencies
  templateUrl: './poll-list.component.html',
  styleUrls: ['./poll-list.component.css']
})
export class PollListComponent implements OnInit {
  polls: Poll[] = [];
  // For single selection polls { pollId: optionIndex | null }
  selectedOption: { [pollId: string]: number | null } = {};
  // For multiple selection polls { pollId: { optionIndex: boolean } }
  selectedOptionsMulti: { [pollId: string]: { [optionIndex: number]: boolean } } = {};
  // Feedback per poll { pollId: message | null }
  feedbackMessage: { [pollId: string]: string | null } = {};

  constructor(private pollService: PollService, private authService: AuthService) { }

  ngOnInit() {
    console.log('PollListComponent - ngOnInit');
    this.refreshPolls();
  }

  // --- Methods for Multiple Selection ---
  onCheckboxChange(event: Event, pollId: string, optionIndex: number) {
    const input = event.target as HTMLInputElement;
    if (!this.selectedOptionsMulti[pollId]) {
      this.selectedOptionsMulti[pollId] = {}; // Initialize if needed
    }
    this.selectedOptionsMulti[pollId][optionIndex] = input.checked;
    console.log(`Multi select state for poll ${pollId}:`, this.selectedOptionsMulti[pollId]);
  }

  hasSelectedMulti(pollId: string): boolean {
    const selections = this.selectedOptionsMulti[pollId];
    // Check if selections exist and if at least one value is true
    return selections && Object.keys(selections).length > 0 && Object.values(selections).some(isSelected => isSelected);
  }
  // --- End Methods for Multiple Selection ---

  vote(event: Event, poll: Poll): void { // Pass the whole poll object
    event.preventDefault(); // Prevent default form submission
    this.feedbackMessage[poll.PollId] = null; // Clear previous feedback

    const userId = this.authService.getUserId();
    if (!userId) {
      console.error('PollListComponent - vote: User ID is missing.');
      this.feedbackMessage[poll.PollId] = 'User ID is missing. Please log in again.';
      // alert('User ID is missing. Please log in again.'); // Less user-friendly
      return;
    }

    let voteData: { pollId: string; userId: string; optionIndex?: number; optionIndices?: number[]; retract: boolean };

    if (poll.AllowMultipleSelections) {
      // --- Handle Multiple Selection Vote ---
      const selectedIndices = Object.entries(this.selectedOptionsMulti[poll.PollId] ?? {})
        .filter(([index, isSelected]) => isSelected)
        .map(([index, isSelected]) => parseInt(index, 10));

      if (selectedIndices.length === 0) {
        console.warn('No options selected for multi-select poll.');
        this.feedbackMessage[poll.PollId] = 'Please select at least one option.';
        return;
      }
      console.log(`Multi-voting for Poll ID: ${poll.PollId}, Options: ${selectedIndices}`);
      voteData = { pollId: poll.PollId, userId, optionIndices: selectedIndices, retract: false };

    } else {
      // --- Handle Single Selection Vote ---
      const singleOptionIndex = this.selectedOption[poll.PollId];
      if (singleOptionIndex === null || singleOptionIndex === undefined) { // Check both null and undefined
        console.warn('No option selected for single-select poll.');
        this.feedbackMessage[poll.PollId] = 'Please select an option.';
        return;
      }
      console.log(`Single-voting for Poll ID: ${poll.PollId}, Option: ${singleOptionIndex}`);
      voteData = { pollId: poll.PollId, userId, optionIndex: singleOptionIndex, retract: false };
    }


    // --- Send Vote to Service ---
    this.pollService.castVote(voteData).subscribe({ // Pass the structured voteData object
      next: (response) => {
        console.log('Vote cast successfully:', response);
        this.feedbackMessage[poll.PollId] = response.message || 'Vote cast successfully!';
        // Clear selections for this poll after successful vote
        if (poll.AllowMultipleSelections) {
          delete this.selectedOptionsMulti[poll.PollId]; // Clear multi-select state
        } else {
          delete this.selectedOption[poll.PollId]; // Clear single-select state
        }
        setTimeout(() => { this.feedbackMessage[poll.PollId] = null; }, 3000); // Clear feedback message
        this.refreshPolls(); // Refresh list to show updated counts (if backend updates them)
      },
      error: (error) => {
        console.error('Error casting vote:', error);
        this.feedbackMessage[poll.PollId] = 'Failed to cast vote. Please try again.';
        // alert('Failed to cast vote. Please try again.'); // Less user-friendly
      }
    });
  }

  private refreshPolls(): void {
    console.log('PollListComponent - refreshPolls');
    this.pollService.getAllPolls().subscribe({
      next: (updatedPolls) => {
        console.log('PollListComponent - polls updated', updatedPolls);
        this.polls = updatedPolls;
        // Initialize selection objects for newly fetched polls
        this.polls.forEach(poll => {
          if (this.selectedOption[poll.PollId] === undefined) {
            this.selectedOption[poll.PollId] = null;
          }
          if (this.selectedOptionsMulti[poll.PollId] === undefined) {
            this.selectedOptionsMulti[poll.PollId] = {};
          }
        });
      },
      error: (error) => console.error('Error fetching polls:', error),
    });
  }
}
