import { Component, OnInit } from '@angular/core';
import { PollService } from '../services/poll.service';
import { Observable } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface Poll {
  id: number;
  question: string;
  options: string[];
  isGlobal: boolean;
}

@Component({
  selector: 'app-poll-list',
  standalone: true,
  imports: [CommonModule, FormsModule], // import FormsModule, as only necessary module to use the template
  templateUrl: './poll-list.component.html',
  styleUrls: ['./poll-list.component.css']
})
export class PollListComponent implements OnInit {
  polls: Poll[] = [];
  selectedOption: { [pollId: number]: number | null } = {}; // Stores the selected option index for each poll

  constructor(private pollService: PollService) { }

  ngOnInit() {
    this.pollService.getAllPolls().subscribe(polls => {
      this.polls = polls;
    });
  }

  vote(event: Event, pollId: number, optionIndex: number | null): void {
    event.preventDefault();
    if (optionIndex === null) {
      console.warn('No option selected.');
      return; 
    }

    this.pollService.castVote(pollId, optionIndex).subscribe({
      next: (response) => {
        console.log('Vote cast successfully:', response);
        this.refreshPolls(); 
      },
      error: (error) => {
        console.error('Error casting vote:', error);
        alert('Failed to cast vote. Please try again.'); 
      }
    });
  }

  private refreshPolls(): void {
    this.pollService.getAllPolls().subscribe({
      next: (updatedPolls) => (this.polls = updatedPolls),
      error: (error) => console.error('Error fetching polls:', error),
    });
  }
}
