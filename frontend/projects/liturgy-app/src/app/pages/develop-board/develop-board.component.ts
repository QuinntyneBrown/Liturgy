import { ChangeDetectionStrategy, Component, DestroyRef, effect, inject, input, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import {
  CdkDrag,
  CdkDragDrop,
  CdkDropList,
  CdkDropListGroup,
} from '@angular/cdk/drag-drop';
import { Board, BoardColumn, BoardRealtime, BoardService, Card } from '@liturgy/api';
import { WorkCardComponent } from '@liturgy/components';

interface ColumnDef {
  readonly key: BoardColumn;
  readonly title: string;
  readonly locked: boolean;
}

const COLUMNS: readonly ColumnDef[] = [
  { key: 'Backlog', title: 'Backlog', locked: false },
  { key: 'InLoop', title: 'In the 5R loop', locked: false },
  { key: 'Review', title: 'Review', locked: false },
  { key: 'Done', title: 'Done', locked: true },
];

@Component({
  selector: 'lit-develop-board',
  imports: [CdkDropListGroup, CdkDropList, CdkDrag, WorkCardComponent],
  templateUrl: './develop-board.component.html',
  styleUrl: './develop-board.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DevelopBoardComponent {
  private readonly boardService = inject(BoardService);
  private readonly realtime = inject(BoardRealtime);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly projectId = input.required<string>();
  readonly board = signal<Board | null>(null);
  readonly columns = COLUMNS;
  readonly rejected = signal<string | null>(null);

  constructor() {
    this.realtime.cardChanged$.pipe(takeUntilDestroyed()).subscribe(() => this.reload());

    effect(() => {
      const id = this.projectId();
      this.load(id);
      void this.realtime.start(id);
    });

    this.destroyRef.onDestroy(() => void this.realtime.stop());
  }

  cardsIn(column: BoardColumn): Card[] {
    return this.board()?.cards.filter((c) => c.column === column) ?? [];
  }

  /** Guards the locked Done column: a card can only enter once its 5R loop is complete. */
  readonly doneEnterPredicate = (drag: CdkDrag<Card>): boolean => drag.data.loggedCount >= 5;
  readonly allowAll = (): boolean => true;

  drop(event: CdkDragDrop<Card[]>, target: BoardColumn): void {
    const card = event.item.data as Card;
    if (event.previousContainer === event.container || card.column === target) {
      return;
    }

    if (target === 'Done' && card.loggedCount < 5) {
      this.flashRejected(card.code);
      return;
    }

    this.boardService.moveCard(card.id, target).subscribe({
      next: () => this.reload(),
      error: () => {
        this.flashRejected(card.code);
        this.reload();
      },
    });
  }

  openCard(card: Card): void {
    void this.router.navigate(['/loop', card.id]);
  }

  private flashRejected(code: string): void {
    this.rejected.set(`${code} can't reach Done until all five movements are logged.`);
  }

  private load(id: string): void {
    this.boardService.get(id).subscribe((board) => this.board.set(board));
  }

  private reload(): void {
    this.load(this.projectId());
  }
}
