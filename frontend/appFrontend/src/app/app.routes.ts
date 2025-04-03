import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { HomeComponent } from './home/home.component';
import { RegisterComponent } from './register/register.component';
import { GroupComponent } from './group/group.component';
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
];