import { Component, OnInit } from '@angular/core';
import { PollService } from '../services/poll.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../services/auth.service';
interface Poll {
  PollId: string;
  UserId: string;
  Question: string;
  Options: string[];
  IsGlobal: boolean;
}

@Component({
  selector: 'app-poll-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './poll-list.component.html',
  styleUrls: ['./poll-list.component.css']
})
export class PollListComponent implements OnInit {
  polls: Poll[] = [];
  selectedOption: { [pollId: string]: number | null } = {};
  feedbackMessage: string | null = null;

  constructor(private pollService: PollService, private authService: AuthService) { }

  ngOnInit() {
    console.log('PollListComponent - ngOnInit');
    this.refreshPolls();
  }

  vote(event: Event, pollId: string, optionIndex: number | null): void {
    event.preventDefault();

    console.log('PollListComponent - vote: Attempting to vote.');
    console.log(`Poll ID: ${pollId}, Option Index: ${optionIndex}`);

    if (optionIndex === null) {
      console.warn('PollListComponent - vote: No option selected.');
      return;
    }

    const userId = this.authService.getUserId();
    console.log(`PollListComponent - vote: User ID: ${userId}`);

    if (!userId) {
      console.error('PollListComponent - vote: User ID is missing. Ensure the user is logged in.');
      alert('User ID is missing. Please log in again.');
      return;
    }

    this.pollService.castVote(pollId, optionIndex, userId).subscribe({
      next: (response) => {
        console.log('PollListComponent - vote: Vote cast successfully:', response.message);
        this.feedbackMessage = response.message;
        setTimeout(() => { this.feedbackMessage = null; }, 3000); // Clear after 3 seconds
        this.refreshPolls();
      },
      error: (error) => {
        console.error('PollListComponent - vote: Error casting vote:', error);
        alert('Failed to cast vote. Please try again.');
      }
    });
  }

  private refreshPolls(): void {
    console.log('PollListComponent - refreshPolls');
    this.pollService.getAllPolls().subscribe({
      next: (updatedPolls) => {
        console.log('PollListComponent - polls updated', updatedPolls);
        this.polls = updatedPolls;
      },
      error: (error) => console.error('Error fetching polls:', error),
    });
  }
}
