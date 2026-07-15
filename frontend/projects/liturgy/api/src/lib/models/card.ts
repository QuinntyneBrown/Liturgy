import { BoardColumn } from './board-column';
import { CardStatus } from './card-status';
import { RKind } from './r-kind';

export interface Card {
  id: string;
  projectId: string;
  sprintId: string;
  code: string;
  title: string;
  description: string | null;
  points: number | null;
  assigneeId: string | null;
  assigneeInitials: string | null;
  column: BoardColumn;
  status: CardStatus;
  currentR: RKind | null;
  isBlocked: boolean;
  loggedCount: number;
}
