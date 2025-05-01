// // src/app/components/poll-list/poll-list.component.ts
// import { Component, OnInit } from '@angular/core';
// import { PollService, Poll } from '../services/poll.service'; // Import Poll interface, verify path
// import { CommonModule, DecimalPipe } from '@angular/common'; // <-- Import DecimalPipe
// import { FormsModule } from '@angular/forms';
// import { AuthService } from '../services/auth.service'; // Verify path

// @Component({
//   selector: 'app-poll-list',
//   standalone: true,
//   imports: [CommonModule, FormsModule, DecimalPipe], // <-- Add DecimalPipe here
//   templateUrl: './poll-list.component.html',
//   styleUrls: ['./poll-list.component.css']
// })
// export class PollListComponent implements OnInit {
//   polls: Poll[] = [];
//   selectedOption: { [pollId: string]: number | null } = {};
//   selectedOptionsMulti: { [pollId: string]: { [optionIndex: number]: boolean } } = {};
//   feedbackMessage: { [pollId: string]: string | null } = {};
//   showResults: { [pollId: string]: boolean } = {}; // Track if results should be shown

//   constructor(private pollService: PollService, private authService: AuthService) { }

//   ngOnInit() {
//     console.log('PollListComponent - ngOnInit');
//     this.refreshPolls();
//   }

//   // --- Methods for Multiple Selection ---
//   onCheckboxChange(event: Event, pollId: string, optionIndex: number) {
//     const input = event.target as HTMLInputElement;
//     if (!this.selectedOptionsMulti[pollId]) {
//       this.selectedOptionsMulti[pollId] = {};
//     }
//     this.selectedOptionsMulti[pollId][optionIndex] = input.checked;
//     console.log(`Multi select state for poll ${pollId}:`, this.selectedOptionsMulti[pollId]);
//   }

//   hasSelectedMulti(pollId: string): boolean {
//     const selections = this.selectedOptionsMulti[pollId];
//     return selections && Object.keys(selections).length > 0 && Object.values(selections).some(isSelected => isSelected);
//   }
//   // --- End Methods for Multiple Selection ---

//   vote(event: Event, poll: Poll): void {
//     event.preventDefault();
//     this.feedbackMessage[poll.PollId] = null;

//     const userId = this.authService.getUserId();
//     if (!userId) {
//       console.error('PollListComponent - vote: User ID is missing.');
//       this.feedbackMessage[poll.PollId] = 'User ID is missing. Please log in again.';
//       return;
//     }

//     let voteData: { pollId: string; userId: string; optionIndex?: number; optionIndices?: number[]; retract: boolean };

//     if (poll.AllowMultipleSelections) {
//       const selectedIndices = Object.entries(this.selectedOptionsMulti[poll.PollId] ?? {})
//         .filter(([_, isSelected]) => isSelected)
//         .map(([index, _]) => parseInt(index, 10));

//       if (selectedIndices.length === 0) {
//         this.feedbackMessage[poll.PollId] = 'Please select at least one option.';
//         return;
//       }
//       voteData = { pollId: poll.PollId, userId, optionIndices: selectedIndices, retract: false };

//     } else {
//       const singleOptionIndex = this.selectedOption[poll.PollId];
//       if (singleOptionIndex === null || singleOptionIndex === undefined) {
//         this.feedbackMessage[poll.PollId] = 'Please select an option.';
//         return;
//       }
//       voteData = { pollId: poll.PollId, userId, optionIndex: singleOptionIndex, retract: false };
//     }

//     this.pollService.castVote(voteData).subscribe({
//       next: (response) => {
//         console.log('Vote cast successfully:', response);
//         this.feedbackMessage[poll.PollId] = response.message || 'Vote cast successfully!';
//         this.showResults[poll.PollId] = true; // Show results after voting

//         // Clear selections for this poll after successful vote
//         if (poll.AllowMultipleSelections) {
//           delete this.selectedOptionsMulti[poll.PollId];
//         } else {
//           delete this.selectedOption[poll.PollId];
//         }
//         setTimeout(() => { this.feedbackMessage[poll.PollId] = null; }, 3000);
//         this.refreshPolls(); // Refresh to get updated counts from backend
//       },
//       error: (error) => {
//         console.error('Error casting vote:', error);
//         // Display specific error from backend if available
//         this.feedbackMessage[poll.PollId] = error.message || 'Failed to cast vote. Please try again.';
//       }
//     });
//   }

//   toggleResults(pollId: string): void {
//     this.showResults[pollId] = !this.showResults[pollId];
//   }

//   private refreshPolls(): void {
//     console.log('PollListComponent - refreshPolls');
//     this.pollService.getAllPolls().subscribe({
//       next: (updatedPolls) => {
//         console.log('PollListComponent - polls updated', updatedPolls);
//         this.polls = updatedPolls;
//         // Initialize/preserve state for fetched polls
//         this.polls.forEach(poll => {
//           if (this.selectedOption[poll.PollId] === undefined) {
//             this.selectedOption[poll.PollId] = null;
//           }
//           if (this.selectedOptionsMulti[poll.PollId] === undefined) {
//             this.selectedOptionsMulti[poll.PollId] = {};
//           }
//           // Keep existing showResults state or default to false
//           if (this.showResults[poll.PollId] === undefined) {
//             this.showResults[poll.PollId] = false; // Default to hiding results initially
//           }
//         });
//       },
//       error: (error) => console.error('Error fetching polls:', error),
//     });
//   }

//   // --- Helper Methods for Results ---

//   /** Gets the vote count for a specific option, handling undefined keys */
//   getVoteCount(poll: Poll, optionIndex: number): number {
//     // Ensure Votes dictionary exists and the key exists
//     return poll.Votes?.[optionIndex] ?? 0;
//   }

//   /** Calculates the total number of votes cast for a poll */
//   getTotalVotes(poll: Poll): number {
//     if (!poll.Votes) {
//       return 0;
//     }
//     // Sum all the values (counts) in the Votes dictionary
//     return Object.values(poll.Votes).reduce((sum, count) => sum + count, 0);
//   }

//   /** Calculates the percentage of votes for a specific option */
//   getVotePercentage(poll: Poll, optionIndex: number): number {
//     const totalVotes = this.getTotalVotes(poll);
//     if (totalVotes === 0) {
//       return 0; // Avoid division by zero
//     }
//     const optionVotes = this.getVoteCount(poll, optionIndex);
//     return (optionVotes / totalVotes) * 100;
//   }
// }
