import { Injectable, inject } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Observable, Subject } from 'rxjs';
import { AuthStateService } from '../auth/auth-state.service';
import { Card } from '../models/card';
import { CardLoop } from '../models/card-loop';
import { Gate } from '../models/gate';
import { Phase } from '../models/phase';
import { API_BASE_URL } from '../services/api-config';
import { BoardRealtime } from './board-realtime';

@Injectable({ providedIn: 'root' })
export class SignalrBoardRealtimeService extends BoardRealtime {
  private readonly baseUrl = inject(API_BASE_URL);
  private readonly auth = inject(AuthStateService);

  private readonly cardChanged = new Subject<Card>();
  private readonly movementLogged = new Subject<CardLoop>();
  private readonly gateChanged = new Subject<Gate>();
  private readonly phaseUnlocked = new Subject<Phase>();

  readonly cardChanged$: Observable<Card> = this.cardChanged.asObservable();
  readonly movementLogged$: Observable<CardLoop> = this.movementLogged.asObservable();
  readonly gateChanged$: Observable<Gate> = this.gateChanged.asObservable();
  readonly phaseUnlocked$: Observable<Phase> = this.phaseUnlocked.asObservable();

  private connection?: HubConnection;
  private projectId?: string;

  async start(projectId: string): Promise<void> {
    await this.stop();
    this.projectId = projectId;

    const connection = new HubConnectionBuilder()
      .withUrl(`${this.baseUrl}/hubs/board`, {
        accessTokenFactory: () => this.auth.token() ?? '',
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on('CardMoved', (card: Card) => this.cardChanged.next(card));
    connection.on('CardCreated', (card: Card) => this.cardChanged.next(card));
    connection.on('CardAssigned', (card: Card) => this.cardChanged.next(card));
    connection.on('MovementLogged', (loop: CardLoop) => this.movementLogged.next(loop));
    connection.on('RequirementToggled', (gate: Gate) => this.gateChanged.next(gate));
    connection.on('GateChanged', (gate: Gate) => this.gateChanged.next(gate));
    connection.on('PhaseUnlocked', (phase: Phase) => this.phaseUnlocked.next(phase));

    connection.onreconnected(() => {
      if (this.projectId) {
        void connection.invoke('JoinProject', this.projectId);
      }
    });

    await connection.start();
    await connection.invoke('JoinProject', projectId);
    this.connection = connection;
  }

  async stop(): Promise<void> {
    if (!this.connection) {
      return;
    }

    const connection = this.connection;
    this.connection = undefined;
    try {
      if (this.projectId) {
        await connection.invoke('LeaveProject', this.projectId);
      }
    } catch {
      // connection may already be closing; ignore
    }
    await connection.stop();
  }
}
