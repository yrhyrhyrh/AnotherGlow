import { Component } from "@angular/core";
import { FormsModule } from "@angular/forms"; // ✅ Import FormsModule
import { User } from "../interfaces/user";

@Component({
  selector: "app-register",
  standalone: true,
  imports: [FormsModule],
  templateUrl: "./register.component.html",
  styleUrls: ["./register.component.css"]
})
export class RegisterComponent {
  user: User = {
    username: "",
    password: "",
  }

  onSubmit() {
    console.log("Username:", this.user.username);
    console.log("Password:", this.user.password);
  }
}
