import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ProjectJourney } from '../models/project-journey';
import { ProjectSummary } from '../models/project-summary';
import { API_BASE_URL } from './api-config';

@Injectable({ providedIn: 'root' })
export class ProjectsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  list(includeClosed = false): Observable<ProjectSummary[]> {
    const query = includeClosed ? '?includeClosed=true' : '';
    return this.http.get<ProjectSummary[]>(`${this.baseUrl}/api/projects${query}`);
  }

  get(projectId: string): Observable<ProjectJourney> {
    return this.http.get<ProjectJourney>(`${this.baseUrl}/api/projects/${projectId}`);
  }

  create(name: string, tag: string): Observable<ProjectSummary> {
    return this.http.post<ProjectSummary>(`${this.baseUrl}/api/projects`, { name, tag });
  }

  update(projectId: string, name: string, tag: string): Observable<ProjectSummary> {
    return this.http.put<ProjectSummary>(`${this.baseUrl}/api/projects/${projectId}`, { name, tag });
  }

  close(projectId: string): Observable<ProjectSummary> {
    return this.http.post<ProjectSummary>(`${this.baseUrl}/api/projects/${projectId}/close`, {});
  }

  reopen(projectId: string): Observable<ProjectSummary> {
    return this.http.post<ProjectSummary>(`${this.baseUrl}/api/projects/${projectId}/reopen`, {});
  }

  delete(projectId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/api/projects/${projectId}`);
  }
}
