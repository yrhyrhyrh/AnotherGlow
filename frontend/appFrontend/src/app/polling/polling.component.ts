import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { PollService } from '../services/poll.service'; // Verify path
import { CommonModule } from '@angular/common';

interface PollResponse { // Keep or modify as needed
  message: string;
}

@Component({
  selector: 'app-polling', // Your poll creation selector
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule], // Keep dependencies
  templateUrl: './polling.component.html', // Your poll creation template
  styleUrls: ['./polling.component.css'] // Your poll creation CSS
})
export class PollingComponent implements OnInit {
  pollForm!: FormGroup;
  successMessage: string | null = null;

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
      isGlobal: [false],
      allowMultipleSelections: [false]
    });
  }

  get options() {
    return this.pollForm.get('options') as FormArray;
  }

  addOption() {
    this.options.push(this.fb.control('', Validators.required));
  }

  removeOption(index: number) {
    if (this.options.length > 2) { // Prevent removing below 2 options
      this.options.removeAt(index);
    } else {
      alert("A poll must have at least two options.");
    }
  }

  onSubmit() {
    this.successMessage = null; // Clear previous message

    if (this.pollForm.valid) {
      console.log("Submitting Poll Data:", this.pollForm.value);

      this.pollService.createPoll(this.pollForm.value)
        .subscribe({
          next: (response: PollResponse) => {
            console.log('Poll created successfully:', response);
            this.successMessage = response.message || "Poll created successfully!";
            this.resetForm();  // Reset the form after success

            // Optionally navigate away after a delay
            setTimeout(() => {
              this.router.navigate(['/polls']); // Navigate to poll list
            }, 2000);
          },
          error: (error: any) => {
            console.error('Error creating poll:', error);
            this.successMessage = "Failed to create poll. Please try again.";
          },
          complete: () => {
            // Completion logic (if needed)
            console.log('Poll creation process completed.');
          }
        });
    } else {
      this.markAllAsTouched(this.pollForm);
      this.successMessage = "Please fill out the form correctly."; // Show validation feedback
    }
  }


  // Reset the form
  resetForm() {
    this.pollForm.reset({ isGlobal: false, allowMultipleSelections: false });
    this.options.clear();  // Clear options array

    // Re-add the initial two options
    this.addOption();
    this.addOption();
  }

  markAllAsTouched(formGroup: FormGroup) {
    Object.values(formGroup.controls).forEach(control => {
      control.markAsTouched();
      if (control instanceof FormGroup) {
        this.markAllAsTouched(control);
      }
      if (control instanceof FormArray) {
        control.controls.forEach(c => c.markAsTouched());
      }
    });
  }
}
