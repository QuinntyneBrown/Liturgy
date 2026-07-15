import { BoardColumn } from './board-column';
import { RKind } from './r-kind';

export interface Card {
  id: string;
  projectId: string;
  sprintId: string;
  code: string;
  title: string;
  assigneeId: string | null;
  assigneeInitials: string | null;
  column: BoardColumn;
  currentR: RKind | null;
  isBlocked: boolean;
  loggedCount: number;
}
