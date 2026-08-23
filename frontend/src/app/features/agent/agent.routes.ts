import { Routes } from '@angular/router';

export const AGENT_ROUTES: Routes = [
  {
    path: 'tickets',
    loadComponent: () =>
      import('./agent-tickets/agent-tickets.component').then((m) => m.AgentTicketsComponent),
  },
  { path: '', redirectTo: 'tickets', pathMatch: 'full' },
];
