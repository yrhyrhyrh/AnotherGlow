import { Component, OnInit } from '@angular/core';
import { PollService } from '../services/poll.service';
import { Observable } from 'rxjs';
import { CommonModule } from '@angular/common';

interface Poll {
    id: number;
    question: string;
    options: string[];
    isGlobal: boolean;
}

@Component({
  selector: 'app-poll-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './poll-list.component.html',
  styleUrls: ['./poll-list.component.css']
})
export class PollListComponent implements OnInit {
  polls: Poll[] = []; // Initialize as an empty array

  constructor(private pollService: PollService) { }

  ngOnInit() {
    this.pollService.getAllPolls().subscribe(polls => {
      this.polls = polls;
    });
  }
}