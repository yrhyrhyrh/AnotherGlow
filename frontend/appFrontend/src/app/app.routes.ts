import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { GroupComponent } from './group/group.component';
import { GroupdetailComponent } from './group/groupdetail/groupdetail.component';
import { PollingComponent } from './polling/polling.component';
import { PollListComponent } from './poll-list/poll-list.component';
import { PostListComponent } from './components/post-list/post-list.component'; // Import PostListComponent
import { AuthGuard } from './auth.guard';

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
    redirectTo: 'groups/general/posts',
    pathMatch: 'full',
    title: "Global Post Wall",     // You can keep the title for documentation, but it's not directly used in redirection
  },
  {
    path: 'groups/:groupId/posts', // Route with groupId parameter
    component: PostListComponent,
    title: "Group Post Wall",
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