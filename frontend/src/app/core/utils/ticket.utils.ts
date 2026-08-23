import { TicketStatus } from '../models';

// Allowed forward/lateral transitions for a support ticket workflow.
const ALLOWED_TRANSITIONS: Record<TicketStatus, TicketStatus[]> = {
  [TicketStatus.Open]: [TicketStatus.InProgress, TicketStatus.Closed],
  [TicketStatus.InProgress]: [TicketStatus.Open, TicketStatus.Resolved, TicketStatus.Closed],
  [TicketStatus.Resolved]: [TicketStatus.InProgress, TicketStatus.Closed],
  [TicketStatus.Closed]: [TicketStatus.Open],
};

export function isValidStatusTransition(
  from: TicketStatus,
  to: TicketStatus
): boolean {
  if (from === to) return true;
  return ALLOWED_TRANSITIONS[from]?.includes(to) ?? false;
}

export function filterTickets<T extends { title: string; description: string }>(
  tickets: T[],
  search: string
): T[] {
  if (!search || !search.trim()) return tickets;
  const term = search.trim().toLowerCase();
  return tickets.filter(
    (t) =>
      t.title.toLowerCase().includes(term) || t.description.toLowerCase().includes(term)
  );
}
