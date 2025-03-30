import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { HomeComponent } from './home/home.component';
import { RegisterComponent } from './register/register.component';
import { AuthGuard } from './auth.guard';
// import { PollingComponent } from './polling/polling.component'; // Remove this direct import

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
    path: 'polls/create',  // The route for creating polls
    loadComponent: () => import('./polling/polling.component').then(mod => mod.PollingComponent), // Lazy-load the standalone component
    title: "Create Poll",
    canActivate: [AuthGuard] // Optional auth guard
  }
];