import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Member } from '../models/member';
import { API_BASE_URL } from './api-config';

@Injectable({ providedIn: 'root' })
export class MembersService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  list(): Observable<Member[]> {
    return this.http.get<Member[]>(`${this.baseUrl}/api/members`);
  }
}
