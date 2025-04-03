import { Component } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { HttpClient } from '@angular/common/http';
import { RouterModule, Router } from '@angular/router';
import { CreateNewGroupComponent } from './create-new-group/create-new-group.component';
import { CommonModule } from "@angular/common";
import { GroupCardComponent } from "./group-card/group-card.component";

@Component({
  selector: "app-group",
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, CreateNewGroupComponent, GroupCardComponent],
  templateUrl: "./group.component.html",
  styleUrls: ["./group.component.css"]
})
export class GroupComponent {
  showDialog = false;
  adminGroups = [
    { name: "Admin Group 1", description: "This is an admin group", groupId: "1" },
    { name: "Admin Group 2", description: "Another admin group", groupId: "2" }
  ];

  nonAdminGroups = [
    { name: "Non-Admin Group 1", description: "Just a regular group", groupId: "3" },
    { name: "Non-Admin Group 2", description: "Another regular group", groupId: "4" }
  ];

  navigateToGroup(groupId: string) {
    console.log("Navigating to group:", groupId);
    // Implement your navigation logic here
  }

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
