import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Decision, SaveDecisionRequest } from '../models/decision';
import { API_BASE_URL } from './api-config';

@Injectable({ providedIn: 'root' })
export class DecisionService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  get(projectId: string): Observable<Decision> {
    return this.http.get<Decision>(`${this.baseUrl}/api/projects/${projectId}/decision`);
  }

  save(projectId: string, request: SaveDecisionRequest): Observable<Decision> {
    return this.http.put<Decision>(`${this.baseUrl}/api/projects/${projectId}/decision`, request);
  }
}
