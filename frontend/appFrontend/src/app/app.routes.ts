import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { HomeComponent } from './home/home.component';
import { RegisterComponent } from './register/register.component';
import { AuthGuard } from './auth.guard';

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
];