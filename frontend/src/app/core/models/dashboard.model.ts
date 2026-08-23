export interface DashboardSummary {
  totalTickets: number;
  countsByStatus: Record<string, number>;
  openAndCriticalCount: number;
  averageResolutionTimeHours: number | null;
  agentWorkload: AgentWorkload[];
}

export interface AgentWorkload {
  agentId: number;
  agentName: string;
  openTicketCount: number;
}
