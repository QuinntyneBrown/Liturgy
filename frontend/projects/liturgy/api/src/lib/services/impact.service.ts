import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Impact } from '../models/impact';
import { API_BASE_URL } from './api-config';

@Injectable({ providedIn: 'root' })
export class ImpactService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  get(projectId: string): Observable<Impact> {
    return this.http.get<Impact>(`${this.baseUrl}/api/projects/${projectId}/impact`);
  }
}
