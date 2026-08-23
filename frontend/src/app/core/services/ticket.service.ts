import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ActivityLogEntry,
  CreateTicketRequest,
  PagedResult,
  Ticket,
  TicketQueryParams,
  UpdateTicketRequest,
} from '../models';

@Injectable({ providedIn: 'root' })
export class TicketService {
  private baseUrl = `${environment.apiUrl}/tickets`;

  constructor(private http: HttpClient) {}

  getTickets(params: TicketQueryParams): Observable<PagedResult<Ticket>> {
    let httpParams = new HttpParams()
      .set('page', params.page)
      .set('pageSize', params.pageSize);
    if (params.status) httpParams = httpParams.set('status', params.status);
    if (params.priority) httpParams = httpParams.set('priority', params.priority);
    if (params.assignedAgentId != null)
      httpParams = httpParams.set('assignedAgentId', params.assignedAgentId);
    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
    if (params.descending != null)
      httpParams = httpParams.set('descending', params.descending);
    return this.http.get<PagedResult<Ticket>>(this.baseUrl, { params: httpParams });
  }

  getTicket(id: number): Observable<Ticket> {
    return this.http.get<Ticket>(`${this.baseUrl}/${id}`);
  }

  createTicket(request: CreateTicketRequest): Observable<Ticket> {
    return this.http.post<Ticket>(this.baseUrl, request);
  }

  updateTicket(id: number, request: UpdateTicketRequest): Observable<Ticket> {
    return this.http.put<Ticket>(`${this.baseUrl}/${id}`, request);
  }

  deleteTicket(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  getActivity(id: number): Observable<ActivityLogEntry[]> {
    return this.http.get<ActivityLogEntry[]>(`${this.baseUrl}/${id}/activity`);
  }
}
