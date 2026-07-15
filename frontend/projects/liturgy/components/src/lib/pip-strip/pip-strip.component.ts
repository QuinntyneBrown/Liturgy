import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { RKind } from '@liturgy/api';

interface Pip {
  readonly label: string;
  readonly done: boolean;
  readonly current: boolean;
}

const PIP_LABELS: ReadonlyArray<{ kind: RKind; short: string }> = [
  { kind: 'Request', short: 'Rq' },
  { kind: 'Receive', short: 'Rc' },
  { kind: 'Review', short: 'Rv' },
  { kind: 'Render', short: 'Rn' },
  { kind: 'Rejoice', short: 'Rj' },
];

/** The compact 5R progress strip shown on a work card. */
@Component({
  selector: 'lit-pip-strip',
  templateUrl: './pip-strip.component.html',
  styleUrl: './pip-strip.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PipStripComponent {
  readonly loggedCount = input.required<number>();
  readonly currentR = input<RKind | null>(null);

  readonly pips = computed<Pip[]>(() => {
    const logged = this.loggedCount();
    return PIP_LABELS.map((pip, i) => ({
      label: pip.short,
      done: i < logged,
      current: pip.kind === this.currentR(),
    }));
  });
}
