import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { UserRole } from './core/models';
import { ShellComponent } from './layout/shell/shell.component';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/register/register.component').then((m) => m.RegisterComponent),
  },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'admin',
        canActivate: [roleGuard],
        data: { roles: [UserRole.Admin] },
        loadChildren: () => import('./features/admin/admin.routes').then((m) => m.ADMIN_ROUTES),
      },
      {
        path: 'agent',
        canActivate: [roleGuard],
        data: { roles: [UserRole.SupportAgent] },
        loadChildren: () => import('./features/agent/agent.routes').then((m) => m.AGENT_ROUTES),
      },
      {
        path: 'customer',
        canActivate: [roleGuard],
        data: { roles: [UserRole.Customer] },
        loadChildren: () =>
          import('./features/customer/customer.routes').then((m) => m.CUSTOMER_ROUTES),
      },
      {
        path: 'tickets',
        loadChildren: () => import('./features/tickets/tickets.routes').then((m) => m.TICKETS_ROUTES),
      },
      {
        path: 'unauthorized',
        loadComponent: () =>
          import('./features/unauthorized/unauthorized.component').then(
            (m) => m.UnauthorizedComponent
          ),
      },
      { path: '', pathMatch: 'full', redirectTo: 'tickets' },
    ],
  },
  {
    path: '**',
    loadComponent: () =>
      import('./features/not-found/not-found.component').then((m) => m.NotFoundComponent),
  },
];
