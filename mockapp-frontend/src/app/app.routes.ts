import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'projects',
    loadComponent: () =>
      import('./projects/project-list/project-list.component').then(
        (m) => m.ProjectListComponent
      ),
  },
  {
    path: 'projects/create',
    loadComponent: () =>
      import('./projects/create-project/create-project.component').then(
        (m) => m.CreateProjectComponent
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
    path: 'projects/:id/mocks/create',
    loadComponent: () =>
      import('./mocks/create-mock/create-mock.component').then(
        (m) => m.CreateMockComponent
      ),
  },
  {
    path: '**',
    redirectTo: 'login',
  },
];
