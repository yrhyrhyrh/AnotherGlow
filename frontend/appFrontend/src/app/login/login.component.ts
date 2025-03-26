import { Component } from "@angular/core";
import { FormsModule } from "@angular/forms"; // ✅ Import FormsModule
import { User } from "../interfaces/user";

@Component({
  selector: "app-login",
  standalone: true,
  imports: [FormsModule],
  templateUrl: "./login.component.html",
  styleUrls: ["./login.component.css"]
})
export class LoginComponent {
  user: User = {
    username: "",
    password: "",
  }

  onSubmit() {
    console.log("Username:", this.user.username);
    console.log("Password:", this.user.password);
  }
}
