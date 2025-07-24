import { Routes } from '@angular/router';
import { authGuard } from '../helpers/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./auth/register/register.component').then(
        (m) => m.RegisterComponent
      ),
  },
  {
    path: 'projects',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./projects/project-list/project-list.component').then(
        (m) => m.ProjectListComponent
      ),
  },
  {
    path: 'projects/:id/mocks',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./mocks/mock-list/mock-list.component').then(
        (m) => m.MockListComponent
      ),
  },
  {
    path: 'payments/success',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./payments/success/success.component').then(
        (p) => p.SuccessComponent
      ),
  },
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'projects',
  },
  {
    path: '**',
    redirectTo: 'projects',
  },
];
