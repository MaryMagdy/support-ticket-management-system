import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateTimeEntryRequest, TimeEntry } from '../models';

@Injectable({ providedIn: 'root' })
export class TimeEntryService {
  constructor(private http: HttpClient) {}

  getTimeEntries(ticketId: number): Observable<TimeEntry[]> {
    return this.http.get<TimeEntry[]>(`${environment.apiUrl}/tickets/${ticketId}/timeentries`);
  }

  addTimeEntry(ticketId: number, request: CreateTimeEntryRequest): Observable<TimeEntry> {
    return this.http.post<TimeEntry>(
      `${environment.apiUrl}/tickets/${ticketId}/timeentries`,
      request
    );
  }
}
