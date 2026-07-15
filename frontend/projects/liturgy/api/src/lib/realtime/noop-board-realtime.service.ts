import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { Card } from '../models/card';
import { CardLoop } from '../models/card-loop';
import { Gate } from '../models/gate';
import { Phase } from '../models/phase';
import { BoardRealtime } from './board-realtime';

/**
 * A realtime that never connects. Used when SignalR is undesirable — notably the
 * Playwright suite with a faked backend, where the app must degrade to REST cleanly.
 */
@Injectable()
export class NoopBoardRealtimeService extends BoardRealtime {
  readonly cardChanged$: Observable<Card> = new Subject<Card>().asObservable();
  readonly movementLogged$: Observable<CardLoop> = new Subject<CardLoop>().asObservable();
  readonly gateChanged$: Observable<Gate> = new Subject<Gate>().asObservable();
  readonly phaseUnlocked$: Observable<Phase> = new Subject<Phase>().asObservable();

  async start(): Promise<void> {
    // no-op
  }

  async stop(): Promise<void> {
    // no-op
  }
}
