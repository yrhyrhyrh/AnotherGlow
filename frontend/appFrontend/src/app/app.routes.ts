import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { HomeComponent } from './home/home.component';
import { RegisterComponent } from './register/register.component';
import { GroupComponent } from './group/group.component';
import { GroupdetailComponent } from './group/groupdetail/groupdetail.component';

import { AuthGuard } from './auth.guard';
import { PollingComponent } from './polling/polling.component';
import { PollListComponent } from './poll-list/poll-list.component'; // Import PollListComponent

export const routes: Routes = [
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
    path: '',
    component: HomeComponent,
    title: "Home Page",
    canActivate: [AuthGuard],
  },
  {
    path: 'group',
    component: GroupComponent,
    title: "Group Page",
    canActivate: [AuthGuard],
  },
  {
    path: 'group/detail/:id',
    component: GroupdetailComponent,
    title: "Group Details",
    canActivate: [AuthGuard],
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
