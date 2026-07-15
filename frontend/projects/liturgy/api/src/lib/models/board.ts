import { Card } from './card';

export interface Board {
  projectId: string;
  sprintId: string;
  sprintNumber: number;
  cards: Card[];
}
