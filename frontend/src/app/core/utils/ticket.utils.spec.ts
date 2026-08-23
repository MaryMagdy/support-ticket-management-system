import { isValidStatusTransition, filterTickets } from './ticket.utils';
import { TicketStatus } from '../models';

describe('isValidStatusTransition', () => {
  it('allows Open -> InProgress', () => {
    expect(isValidStatusTransition(TicketStatus.Open, TicketStatus.InProgress)).toBeTrue();
  });

  it('allows Open -> Closed', () => {
    expect(isValidStatusTransition(TicketStatus.Open, TicketStatus.Closed)).toBeTrue();
  });

  it('disallows Open -> Resolved directly', () => {
    expect(isValidStatusTransition(TicketStatus.Open, TicketStatus.Resolved)).toBeFalse();
  });

  it('allows InProgress -> Resolved', () => {
    expect(isValidStatusTransition(TicketStatus.InProgress, TicketStatus.Resolved)).toBeTrue();
  });

  it('disallows Closed -> Resolved', () => {
    expect(isValidStatusTransition(TicketStatus.Closed, TicketStatus.Resolved)).toBeFalse();
  });

  it('allows Closed -> Open (reopen)', () => {
    expect(isValidStatusTransition(TicketStatus.Closed, TicketStatus.Open)).toBeTrue();
  });

  it('treats same-status transition as valid (no-op)', () => {
    expect(isValidStatusTransition(TicketStatus.Resolved, TicketStatus.Resolved)).toBeTrue();
  });
});

describe('filterTickets', () => {
  const tickets = [
    { title: 'Printer jam', description: 'The printer on floor 2 is jammed' },
    { title: 'VPN issue', description: 'Cannot connect to VPN from home' },
    { title: 'Laptop request', description: 'Need a new laptop for onboarding' },
  ];

  it('returns all tickets when search is empty', () => {
    expect(filterTickets(tickets, '')).toEqual(tickets);
  });

  it('filters by title case-insensitively', () => {
    const result = filterTickets(tickets, 'printer');
    expect(result.length).toBe(1);
    expect(result[0].title).toBe('Printer jam');
  });

  it('filters by description text', () => {
    const result = filterTickets(tickets, 'VPN');
    expect(result.length).toBe(1);
    expect(result[0].title).toBe('VPN issue');
  });

  it('returns empty array when nothing matches', () => {
    expect(filterTickets(tickets, 'nonexistent')).toEqual([]);
  });
});
