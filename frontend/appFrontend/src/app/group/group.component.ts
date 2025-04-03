import { Component } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { HttpClient } from '@angular/common/http';
import { RouterModule, Router } from '@angular/router';
import { CreateNewGroupComponent } from './create-new-group/create-new-group.component';
import { CommonModule } from "@angular/common";

@Component({
  selector: "app-group",
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, CreateNewGroupComponent],
  templateUrl: "./group.component.html",
  styleUrls: ["./group.component.css"]
})
export class GroupComponent {
  showDialog = false;

  openDialog() {
    this.showDialog = true;
  }

  closeDialog() {
    this.showDialog = false;
  }

  handleGroupCreation(groupName: string) {
    console.log("New group created:", groupName);
    this.closeDialog();
  }
}
