import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Comment, CreateCommentRequest } from '../models';

@Injectable({ providedIn: 'root' })
export class CommentService {
  constructor(private http: HttpClient) {}

  getComments(ticketId: number): Observable<Comment[]> {
    return this.http.get<Comment[]>(`${environment.apiUrl}/tickets/${ticketId}/comments`);
  }

  addComment(ticketId: number, request: CreateCommentRequest): Observable<Comment> {
    return this.http.post<Comment>(`${environment.apiUrl}/tickets/${ticketId}/comments`, request);
  }
}
