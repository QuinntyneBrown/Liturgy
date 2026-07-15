import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Invitation, InvitationPreview } from '../models/invitation';
import { API_BASE_URL } from './api-config';

@Injectable({ providedIn: 'root' })
export class InvitationsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  /** Pending invitations for the caller's workspace. */
  list(): Observable<Invitation[]> {
    return this.http.get<Invitation[]>(`${this.baseUrl}/api/invitations`);
  }

  /** Lead invites someone by email. */
  create(email: string, role: string | null = null): Observable<Invitation> {
    return this.http.post<Invitation>(`${this.baseUrl}/api/invitations`, { email, role });
  }

  /** Anonymous preview of a pending invitation, for the sign-up screen. */
  getByToken(token: string): Observable<InvitationPreview> {
    return this.http.get<InvitationPreview>(`${this.baseUrl}/api/invitations/${token}`);
  }

  /** An authenticated existing user accepts a pending invitation. */
  accept(token: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/api/invitations/${token}/accept`, {});
  }

  /** A Lead revokes a pending invitation. */
  revoke(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/api/invitations/${id}`);
  }
}
