import { ChangeDetectionStrategy, Component, DestroyRef, ElementRef, computed, effect, inject, input, signal, viewChild } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import {
  CdkDrag,
  CdkDragDrop,
  CdkDropList,
  CdkDropListGroup,
} from '@angular/cdk/drag-drop';
import { Board, BoardColumn, BoardRealtime, BoardService, Card, Member, MembersService } from '@liturgy/api';
import { CardAssign, WorkCardComponent } from '@liturgy/components';
import { RailContextService } from '../../shell/rail-context.service';

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
  imports: [CdkDropListGroup, CdkDropList, CdkDrag, WorkCardComponent, RouterLink],
  templateUrl: './develop-board.component.html',
  styleUrl: './develop-board.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DevelopBoardComponent {
  private readonly boardService = inject(BoardService);
  private readonly membersService = inject(MembersService);
  private readonly realtime = inject(BoardRealtime);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly rail = inject(RailContextService);

  readonly projectId = input.required<string>();
  readonly board = signal<Board | null>(null);
  readonly members = signal<Member[]>([]);
  readonly columns = COLUMNS;
  readonly rejected = signal<string | null>(null);
  readonly adding = signal(false);
  private readonly titleInput = viewChild.required<ElementRef<HTMLInputElement>>('titleInput');

  /** The card whose 5R loop the rail nest and "Open 5R loop" CTA focus on. */
  readonly focusCard = computed<Card | null>(() => {
    const cards = this.board()?.cards ?? [];
    const inLoop = cards.filter((c) => c.column === 'InLoop');
    return inLoop.find((c) => c.loggedCount < 5) ?? inLoop[0] ?? cards[0] ?? null;
  });

  /** Average logged R movements across the board, e.g. "3.4". */
  readonly avgR = computed(() => {
    const cards = this.board()?.cards ?? [];
    if (cards.length === 0) {
      return '0';
    }
    const total = cards.reduce((sum, c) => sum + c.loggedCount, 0);
    return (total / cards.length).toFixed(1);
  });

  constructor() {
    this.realtime.cardChanged$.pipe(takeUntilDestroyed()).subscribe(() => this.reload());

    effect(() => {
      const id = this.projectId();
      this.load(id);
      void this.realtime.start(id);
    });

    this.membersService.list().subscribe({
      next: (members) => this.members.set(members),
      error: () => this.members.set([]),
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

  addCard(): void {
    const el = this.titleInput().nativeElement;
    const title = el.value.trim();
    if (!title || this.adding()) {
      return;
    }
    this.adding.set(true);
    this.boardService.createCard(this.projectId(), title).subscribe({
      next: (created) => {
        el.value = '';
        this.adding.set(false);
        // Show the new card immediately, then reconcile from the server.
        this.board.update((b) => (b ? { ...b, cards: [...b.cards, created] } : b));
        this.reload();
      },
      error: () => this.adding.set(false),
    });
  }

  assign(event: CardAssign): void {
    this.boardService.assignCard(event.card.id, event.assigneeId).subscribe({
      next: () => this.reload(),
    });
  }

  openCard(card: Card): void {
    void this.router.navigate(['/loop', card.id]);
  }

  private flashRejected(code: string): void {
    this.rejected.set(`${code} can't reach Done until all five movements are logged.`);
  }

  private load(id: string): void {
    this.boardService.get(id).subscribe((board) => {
      this.board.set(board);
      this.rail.showProject(id, this.focusCard()?.loggedCount ?? 0);
    });
  }

  private reload(): void {
    this.load(this.projectId());
  }
}
