import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { PollService, Poll } from '../services/poll.service';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-poll',
  standalone: true,
  imports: [CommonModule, FormsModule, DecimalPipe],
  templateUrl: './poll.component.html',
  styleUrls: ['./poll.component.css']
})
export class PollComponent implements OnInit, OnDestroy {
  @Input() poll!: Poll;
  selectedOption: number | null = null;
  selectedOptionsMulti: { [optionIndex: number]: boolean } = {};
  feedbackMessage: string | null = null;
  showResults = false;
  private pollChangeSub!: Subscription;

  constructor(private pollService: PollService, private authService: AuthService) { }

  ngOnInit(): void {
    this.pollChangeSub = this.pollService.pollChanged$.subscribe(() => {
      console.log("🔁 PollComponent received change signal, refreshing poll");
      if (this.poll?.PollId) {
        this.pollService.getPollById(this.poll.PollId).subscribe({
          next: (updatedPoll) => {
            this.poll = updatedPoll;
          },
          error: (err) => {
            console.error('Error refreshing poll after notification:', err);
          }
        });
      }
    });
  }

  ngOnDestroy(): void {
    if (this.pollChangeSub) {
      this.pollChangeSub.unsubscribe();
    }
  }

  onCheckboxChange(event: Event, optionIndex: number) {
    const input = event.target as HTMLInputElement;
    this.selectedOptionsMulti[optionIndex] = input.checked;
  }

  hasSelectedMulti(): boolean {
    return Object.values(this.selectedOptionsMulti).some((val) => val);
  }

  vote(event: Event): void {
    event.preventDefault();
    this.feedbackMessage = null;

    const userId = this.authService.getUserId();
    if (!userId) {
      this.feedbackMessage = 'User ID is missing. Please log in again.';
      return;
    }

    let voteData: { pollId: string; userId: string; optionIndex?: number; optionIndices?: number[]; retract: boolean };

    if (this.poll.AllowMultipleSelections) {
      const selectedIndices = Object.entries(this.selectedOptionsMulti)
        .filter(([_, isSelected]) => isSelected)
        .map(([index, _]) => parseInt(index, 10));

      if (selectedIndices.length === 0) {
        this.feedbackMessage = 'Please select at least one option.';
        return;
      }

      voteData = { pollId: this.poll.PollId ?? "", userId, optionIndices: selectedIndices, retract: false };
    } else {
      if (this.selectedOption === null || this.selectedOption === undefined) {
        this.feedbackMessage = 'Please select an option.';
        return;
      }
      voteData = { pollId: this.poll.PollId ?? "", userId, optionIndex: this.selectedOption, retract: false };
    }

    this.pollService.castVote(voteData).subscribe({
      next: (response) => {
        this.feedbackMessage = response.message || 'Vote cast successfully!';
        this.showResults = true;
        if (this.poll.AllowMultipleSelections) {
          this.selectedOptionsMulti = {};
        } else {
          this.selectedOption = null;
        }
        this.pollService.getPollById(this.poll.PollId!).subscribe({
          next: (updatedPoll) => {
            this.poll = updatedPoll;
          },
          error: (err) => {
            console.error('Failed to refresh poll:', err);
          }
        });
        setTimeout(() => { this.feedbackMessage = null; }, 3000);
      },
      error: (error) => {
        this.feedbackMessage = error.message || 'Failed to cast vote. Please try again.';
      }
    });
  }

  getVoteCount(optionIndex: number): number {
    return this.poll.Votes?.[optionIndex] ?? 0;
  }

  getTotalVotes(): number {
    return Object.values(this.poll.Votes ?? {}).reduce((sum, count) => sum + count, 0);
  }

  getVotePercentage(optionIndex: number): number {
    const total = this.getTotalVotes();
    return total ? (this.getVoteCount(optionIndex) / total) * 100 : 0;
  }

  toggleResults(): void {
    this.showResults = !this.showResults;
  }
}
