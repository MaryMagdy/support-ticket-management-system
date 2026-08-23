export interface TimeEntry {
  id: number;
  ticketId: number;
  userId: number;
  userName: string;
  workDate: string;
  durationMinutes: number;
  description?: string | null;
  createdAt: string;
}

export interface CreateTimeEntryRequest {
  workDate: string;
  durationMinutes: number;
  description?: string | null;
}
