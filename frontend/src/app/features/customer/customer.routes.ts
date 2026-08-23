import { Routes } from '@angular/router';

export const CUSTOMER_ROUTES: Routes = [
  {
    path: 'tickets',
    loadComponent: () =>
      import('./customer-tickets/customer-tickets.component').then(
        (m) => m.CustomerTicketsComponent
      ),
  },
  {
    path: 'tickets/new',
    loadComponent: () =>
      import('./create-ticket/create-ticket.component').then((m) => m.CreateTicketComponent),
  },
  { path: '', redirectTo: 'tickets', pathMatch: 'full' },
];
