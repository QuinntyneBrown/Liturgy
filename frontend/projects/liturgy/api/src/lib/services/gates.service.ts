import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Gate } from '../models/gate';
import { API_BASE_URL } from './api-config';

@Injectable({ providedIn: 'root' })
export class GatesService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  toggleRequirement(requirementId: string, done: boolean): Observable<Gate> {
    return this.http.post<Gate>(
      `${this.baseUrl}/api/gates/requirements/${requirementId}/toggle`,
      { done },
    );
  }
}
