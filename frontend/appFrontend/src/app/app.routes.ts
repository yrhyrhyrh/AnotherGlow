import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { HomeComponent } from './home/home.component';
import { RegisterComponent } from './register/register.component';
import { AuthGuard } from './auth.guard';
import { PollingComponent } from './polling/polling.component';
import { PollListComponent } from './poll-list/poll-list.component'; // Import PollListComponent

export const routes: Routes = [
  {
    path: '',
    component: HomeComponent,
    title: "Home Page",
    canActivate: [AuthGuard],
  },
  {
    path: 'login',
    component: LoginComponent,
    title: "Login Page",
  },
  {
    path: 'register',
    component: RegisterComponent,
    title: "Registration Page",
  },
  {
    path: 'polls/create',
    component: PollingComponent,
    title: "Create Poll",
    canActivate: [AuthGuard]
  },
  {
    path: 'polls', // Route to display polls
    component: PollListComponent,
    title: "Poll List",
    canActivate: [AuthGuard] // Optional auth guard
  }
];