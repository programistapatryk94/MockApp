import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'mocks',
    loadComponent: () =>
      import('./mocks/mock-list/mock-list.component').then(
        (m) => m.MockListComponent
      ),
  },
  {
    path: 'mocks/create',
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
