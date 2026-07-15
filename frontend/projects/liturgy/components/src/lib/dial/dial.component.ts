import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

interface DialSegment {
  readonly dash: string;
  readonly offset: number;
  readonly color: string;
}

const R_LABELS = ['Request', 'Receive', 'Review', 'Render', 'Rejoice'];

/**
 * The canonical-hours 5R dial — a ring of five arc segments, `filled` of them lit.
 * Ported from the mock's `buildDial` SVG routine into a real, testable component.
 */
@Component({
  selector: 'lit-dial',
  templateUrl: './dial.component.html',
  styleUrl: './dial.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DialComponent {
  readonly filled = input.required<number>();
  readonly size = input(120);
  readonly label = input('5R loop');
  readonly onPaper = input(false);

  private readonly radius = 48;
  readonly center = 60;

  readonly segments = computed<DialSegment[]>(() => {
    const circumference = 2 * Math.PI * this.radius;
    const seg = circumference / 5;
    const gap = 6;
    const filled = this.filled();
    const trackColor = this.onPaper() ? 'var(--ink-line)' : 'var(--ink-line)';

    return Array.from({ length: 5 }, (_, i) => ({
      dash: `${seg - gap} ${circumference - seg + gap}`,
      offset: -i * seg,
      color: i < filled ? 'var(--lime)' : i === filled ? 'var(--develop)' : trackColor,
    }));
  });

  readonly r = this.radius;

  readonly ariaLabel = computed(
    () => `${this.filled()} of 5 movements complete: ${R_LABELS.slice(0, this.filled()).join(', ')}`,
  );
}
