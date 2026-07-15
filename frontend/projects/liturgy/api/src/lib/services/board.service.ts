import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Board } from '../models/board';
import { BoardColumn } from '../models/board-column';
import { Card } from '../models/card';
import { API_BASE_URL } from './api-config';

@Injectable({ providedIn: 'root' })
export class BoardService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  get(projectId: string): Observable<Board> {
    return this.http.get<Board>(`${this.baseUrl}/api/board/${projectId}`);
  }

  createCard(projectId: string, title: string, assigneeId: string | null = null): Observable<Card> {
    return this.http.post<Card>(`${this.baseUrl}/api/board/cards`, { projectId, title, assigneeId });
  }

  moveCard(cardId: string, column: BoardColumn): Observable<Card> {
    return this.http.post<Card>(`${this.baseUrl}/api/board/cards/${cardId}/move`, { column });
  }

  assignCard(cardId: string, assigneeId: string | null): Observable<Card> {
    return this.http.post<Card>(`${this.baseUrl}/api/board/cards/${cardId}/assign`, { assigneeId });
  }
}
