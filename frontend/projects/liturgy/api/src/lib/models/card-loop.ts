import { BoardColumn } from './board-column';
import { Movement } from './movement';
import { RKind } from './r-kind';

export interface CardLoop {
  cardId: string;
  code: string;
  title: string;
  column: BoardColumn;
  currentR: RKind | null;
  loggedCount: number;
  canMarkDone: boolean;
  movements: Movement[];
}
