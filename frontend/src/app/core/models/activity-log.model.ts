export enum ActivityType {
  StatusChange = 'StatusChange',
  PriorityChange = 'PriorityChange',
  AssignmentChange = 'AssignmentChange',
  CommentAdded = 'CommentAdded',
}

export interface ActivityLogEntry {
  id: number;
  ticketId: number;
  userId: number;
  userName: string;
  type: ActivityType;
  oldValue?: string | null;
  newValue?: string | null;
  createdAt: string;
}
