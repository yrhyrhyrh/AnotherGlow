import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { GroupdetailComponent } from './group/groupdetail/groupdetail.component';
import { PollingComponent } from './polling/polling.component';
import { PollListComponent } from './poll-list/poll-list.component';
import { PostListComponent } from './components/post-list/post-list.component';
import { AuthGuard } from './auth.guard';
import { MainLayoutComponent } from './layout/main-layout.component';
import { CreateGroupComponent } from './group/create-group/create-group.component';

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
    component: MainLayoutComponent,
    canActivate: [AuthGuard],
    children: [
      {
        path: '',
        redirectTo: 'groups/general/posts',
        pathMatch: 'full',
      },
      {
        path: 'groups/:groupId/posts',
        component: PostListComponent,
        title: "Group Post Wall",
        data: { renderMode: 'client' }
      },
      {
        path: 'group/create',
        component: CreateGroupComponent,
        title: "Create New Group",
      },
      {
        path: 'group/detail/:id',
        component: GroupdetailComponent,
        title: "Group Details",
        data: { renderMode: 'client' }
      },
      {
        path: 'polls/create',
        component: PollingComponent,
        title: "Create Poll",
      },
      {
        path: 'polls',
        component: PollListComponent,
        title: "Poll List",
      }
    ]
  }
];