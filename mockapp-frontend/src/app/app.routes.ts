import { Routes } from '@angular/router';

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
    loadComponent: () =>
      import('./projects/project-list/project-list.component').then(
        (m) => m.ProjectListComponent
      ),
  },
  {
    path: 'projects/:id/mocks',
    loadComponent: () =>
      import('./mocks/mock-list/mock-list.component').then(
        (m) => m.MockListComponent
      ),
  },
  {
    path: '**',
    redirectTo: 'projects',
  },
];
