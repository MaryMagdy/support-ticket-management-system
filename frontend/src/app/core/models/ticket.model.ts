import { TicketPriority, TicketStatus } from './enums';

export interface Ticket {
  id: number;
  title: string;
  description: string;
  status: TicketStatus;
  priority: TicketPriority;
  customerId: number;
  customerName?: string | null;
  assignedAgentId?: number | null;
  assignedAgentName?: string | null;
  createdAt: string;
  updatedAt: string;
  totalTimeMinutes: number;
}

export interface CreateTicketRequest {
  title: string;
  description: string;
  priority: TicketPriority;
}

export interface UpdateTicketRequest {
  title?: string;
  description?: string;
  status?: TicketStatus;
  priority?: TicketPriority;
  assignedAgentId?: number | null;
}

export interface TicketQueryParams {
  page: number;
  pageSize: number;
  status?: TicketStatus | null;
  priority?: TicketPriority | null;
  assignedAgentId?: number | null;
  search?: string | null;
  sortBy?: string | null;
  descending?: boolean | null;
}
