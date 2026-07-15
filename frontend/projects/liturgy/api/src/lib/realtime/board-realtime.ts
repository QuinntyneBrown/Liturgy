import { Observable } from 'rxjs';
import { Card } from '../models/card';
import { CardLoop } from '../models/card-loop';
import { Gate } from '../models/gate';
import { Phase } from '../models/phase';

/**
 * Real-time collaboration surface for a project. Used as the DI token so the app can
 * bind the SignalR implementation while tests bind a fake. Streams mirror the server
 * hub events; `cardChanged$` covers CardMoved / CardCreated / CardAssigned / CardUpdated,
 * `cardDeleted$` carries the id of a removed card, and `gateChanged$` covers
 * RequirementToggled / GateChanged.
 */
export abstract class BoardRealtime {
  abstract readonly cardChanged$: Observable<Card>;
  abstract readonly cardDeleted$: Observable<string>;
  abstract readonly movementLogged$: Observable<CardLoop>;
  abstract readonly gateChanged$: Observable<Gate>;
  abstract readonly phaseUnlocked$: Observable<Phase>;

  abstract start(projectId: string): Promise<void>;
  abstract stop(): Promise<void>;
}
