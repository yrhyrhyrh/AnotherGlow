import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { PollService } from '../services/poll.service';
import { CommonModule } from '@angular/common';

interface PollResponse {
  message: string;
  // Add other properties that are part of your success response from the backend
}

@Component({
  selector: 'app-polling',
  standalone: true, // Add standalone: true
  imports: [ReactiveFormsModule, CommonModule], // Add the import of the modules
  templateUrl: './polling.component.html',
  styleUrls: ['./polling.component.css']
})
export class PollingComponent implements OnInit {
  pollForm!: FormGroup;
  successMessage: string | null = null; // Add this line

  constructor(
    private fb: FormBuilder,
    private pollService: PollService,
    private router: Router
  ) { }

  ngOnInit() {
    this.pollForm = this.fb.group({
      question: ['', Validators.required],
      options: this.fb.array([
        this.fb.control('', Validators.required),
        this.fb.control('', Validators.required)
      ], Validators.minLength(2)),
      isGlobal: [false]
    });
  }

  get options() {
    return this.pollForm.get('options') as FormArray;
  }

  addOption() {
    this.options.push(this.fb.control('', Validators.required));
  }

  removeOption(index: number) {
    this.options.removeAt(index);
  }

  onSubmit() {
    if (this.pollForm.valid) {
      this.pollService.createPoll(this.pollForm.value)
        .subscribe(
          (response: PollResponse) => {
            console.log('Poll created successfully:', response);
            this.successMessage = response.message; // Set the success message
            // Handle success (e.g., display a success message, navigate to a different page)
          },
          (error: any) => {
            console.error('Error creating poll:', error);
            // Handle the error (e.g., display an error message)
          }
        );
    } else {
      // Mark all controls as touched to display validation errors
      this.markAllAsTouched(this.pollForm);
    }
  }

  markAllAsTouched(formGroup: FormGroup) {
    Object.values(formGroup.controls).forEach(control => {
      control.markAsTouched();

      if (control instanceof FormGroup) {
        this.markAllAsTouched(control);
      }
    });
  }
}
