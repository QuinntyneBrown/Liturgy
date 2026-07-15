import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Card } from '../models/card';
import { CardLoop } from '../models/card-loop';
import { LogMovementRequest } from '../models/log-movement-request';
import { API_BASE_URL } from './api-config';

@Injectable({ providedIn: 'root' })
export class LoopService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  get(cardId: string): Observable<CardLoop> {
    return this.http.get<CardLoop>(`${this.baseUrl}/api/loop/cards/${cardId}`);
  }

  logMovement(cardId: string, request: LogMovementRequest): Observable<CardLoop> {
    return this.http.post<CardLoop>(`${this.baseUrl}/api/loop/cards/${cardId}/movements`, request);
  }

  markDone(cardId: string): Observable<Card> {
    return this.http.post<Card>(`${this.baseUrl}/api/loop/cards/${cardId}/done`, {});
  }
}
