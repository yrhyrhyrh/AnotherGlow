import { Component, EventEmitter, OnInit, Output } from '@angular/core';
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
  @Output() pollDataChanged = new EventEmitter<any>();
  pollForm!: FormGroup;
  successMessage: string | null = null;

  constructor(
    private fb: FormBuilder,
    private pollService: PollService,
    private router: Router
  ) { }

  ngOnInit() {
    this.pollForm = this.fb.group({
      Question: ['', Validators.required],
      Options: this.fb.array([
        this.fb.control('', Validators.required),
        this.fb.control('', Validators.required)
      ], Validators.minLength(2)),
      IsGlobal: [false],
      AllowMultipleSelections: [false]
    });
  
    // Emit the initial empty form
    this.pollForm.valueChanges.subscribe(value => {
      this.pollDataChanged.emit(value);
    });
  }
  

  get Options() {
    return this.pollForm.get('Options') as FormArray;
  }

  addOption() {
    this.Options.push(this.fb.control('', Validators.required));
  }

  removeOption(index: number) {
    if (this.Options.length > 2) { // Prevent removing below 2 Options
      this.Options.removeAt(index);
    } else {
      alert("A poll must have at least two Options.");
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
            // this.resetForm();  // Reset the form after success
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
    this.pollForm.reset({ IsGlobal: false, AllowMultipleSelections: false });
    this.Options.clear();  // Clear Options array

    // Re-add the initial two Options
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
