import { Component, OnInit } from '@angular/core';
import { PollService } from '../services/poll.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface Poll {
  pollId: string;
  userId: string;
  question: string;
  options: string[];
  isGlobal: boolean;
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

  constructor(private pollService: PollService) { }

  ngOnInit() {
    console.log('PollListComponent - ngOnInit');
    this.refreshPolls();
  }

  vote(event: Event, pollId: string, optionIndex: number | null): void {
    event.preventDefault();

    if (optionIndex === null) {
      console.warn('No option selected.');
      return;
    }

    const userId = localStorage.getItem('userId');

    if (!userId) {
      console.error('User ID is missing. Ensure the user is logged in.');
      alert('User ID is missing. Please log in again.');
      return;
    }

    this.pollService.castVote(pollId, optionIndex, userId).subscribe({
      next: (response) => {
        console.log('Vote cast successfully:', response.message);
        this.feedbackMessage = response.message;
        this.refreshPolls(); // Optional: Refresh the polls after voting
      },
      error: (error) => {
        console.error('Error casting vote:', error);
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
