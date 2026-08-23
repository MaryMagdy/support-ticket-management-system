import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { User, UserRole } from '../models';

export interface CreateUserRequest {
  fullName: string;
  email: string;
  password: string;
  role: string;
}

export interface UpdateUserRequest {
  fullName?: string;
  email?: string;
  role?: string;
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private baseUrl = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  /** Backend returns a plain array (not paginated) — optionally filter by role. */
  getUsers(role?: UserRole): Observable<User[]> {
    let params = new HttpParams();
    if (role) params = params.set('role', role);
    return this.http.get<User[]>(this.baseUrl, { params });
  }

  getUser(id: number): Observable<User> {
    return this.http.get<User>(`${this.baseUrl}/${id}`);
  }

  createUser(request: CreateUserRequest): Observable<User> {
    return this.http.post<User>(this.baseUrl, request);
  }

  updateUser(id: number, request: UpdateUserRequest): Observable<User> {
    return this.http.put<User>(`${this.baseUrl}/${id}`, request);
  }

  deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
