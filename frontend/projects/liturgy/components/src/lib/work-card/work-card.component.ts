import { ChangeDetectionStrategy, Component, input, output, signal } from '@angular/core';
import { Card, Member } from '@liturgy/api';
import { PipStripComponent } from '../pip-strip/pip-strip.component';

export interface CardAssign {
  card: Card;
  assigneeId: string | null;
}

export interface CardPoint {
  card: Card;
  points: number | null;
}

/** Presentational Kanban card: id, title, description, points, 5R pips, assignee, lifecycle actions. */
@Component({
  selector: 'lit-work-card',
  imports: [PipStripComponent],
  templateUrl: './work-card.component.html',
  styleUrl: './work-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WorkCardComponent {
  readonly card = input.required<Card>();
  /** When provided, the card exposes an assignee selector. */
  readonly members = input<Member[]>([]);
  readonly open = output<Card>();
  readonly assign = output<CardAssign>();
  readonly point = output<CardPoint>();
  readonly cancel = output<Card>();
  readonly close = output<Card>();
  readonly reopen = output<Card>();
  readonly remove = output<Card>();

  readonly menuOpen = signal(false);

  toggleMenu(): void {
    this.menuOpen.update((v) => !v);
  }

  onAssign(value: string): void {
    this.assign.emit({ card: this.card(), assigneeId: value || null });
  }

  onPoint(value: string): void {
    const trimmed = value.trim();
    const points = trimmed === '' ? null : Number(trimmed);
    this.point.emit({ card: this.card(), points: Number.isFinite(points as number) ? points : null });
  }

  emitAndClose(action: 'cancel' | 'close' | 'reopen' | 'remove'): void {
    this.menuOpen.set(false);
    this[action].emit(this.card());
  }
}
