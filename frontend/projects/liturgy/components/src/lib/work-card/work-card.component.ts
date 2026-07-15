import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { Card, Member } from '@liturgy/api';
import { PipStripComponent } from '../pip-strip/pip-strip.component';

export interface CardAssign {
  card: Card;
  assigneeId: string | null;
}

/** Presentational Kanban card: id, title, 5R pips, assignee avatar (+ optional assign control). */
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

  onAssign(value: string): void {
    this.assign.emit({ card: this.card(), assigneeId: value || null });
  }
}
