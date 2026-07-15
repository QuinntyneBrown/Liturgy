import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthResult } from '../models/auth-result';
import { CurrentUser } from '../models/current-user';
import { API_BASE_URL } from './api-config';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  register(
    email: string,
    firstName: string,
    lastName: string,
    password: string,
    invitationToken: string | null = null,
  ): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${this.baseUrl}/api/auth/register`, {
      email,
      firstName,
      lastName,
      password,
      invitationToken,
    });
  }

  signIn(email: string, password: string): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${this.baseUrl}/api/auth/sign-in`, { email, password });
  }

  me(): Observable<CurrentUser> {
    return this.http.get<CurrentUser>(`${this.baseUrl}/api/me`);
  }
}
