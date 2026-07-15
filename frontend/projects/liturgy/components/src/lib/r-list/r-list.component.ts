import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

const R_NAMES = ['Request', 'Receive', 'Review', 'Render', 'Rejoice'] as const;

type RListState = 'done' | 'current' | 'locked';

interface RListItem {
  readonly name: string;
  readonly index: number;
  readonly state: RListState;
}

/**
 * The vertical 5R list — the third canonical representation of loop progress
 * (alongside the SVG dial and the horizontal pip strip). `filled` movements are
 * Done, the next is Current, the rest Locked, matching the dial and pip strip.
 */
@Component({
  selector: 'lit-rlist',
  templateUrl: './r-list.component.html',
  styleUrl: './r-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RListComponent {
  readonly filled = input.required<number>();

  readonly items = computed<RListItem[]>(() => {
    const filled = this.filled();
    return R_NAMES.map((name, i) => ({
      name,
      index: i + 1,
      state: i < filled ? 'done' : i === filled ? 'current' : 'locked',
    }));
  });
}
