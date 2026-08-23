export interface Comment {
  id: number;
  ticketId: number;
  userId: number;
  userName: string;
  text: string;
  createdAt: string;
}

export interface CreateCommentRequest {
  text: string;
}
