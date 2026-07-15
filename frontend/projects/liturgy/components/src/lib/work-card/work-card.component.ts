import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { Card } from '@liturgy/api';
import { PipStripComponent } from '../pip-strip/pip-strip.component';

/** Presentational Kanban card: id, title, 5R pips, assignee avatar. */
@Component({
  selector: 'lit-work-card',
  imports: [PipStripComponent],
  templateUrl: './work-card.component.html',
  styleUrl: './work-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WorkCardComponent {
  readonly card = input.required<Card>();
  readonly open = output<Card>();
}
