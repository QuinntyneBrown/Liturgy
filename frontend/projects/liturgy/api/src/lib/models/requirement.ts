import { RequirementState } from './requirement-state';

export interface Requirement {
  id: string;
  label: string;
  meta: string | null;
  state: RequirementState;
  order: number;
}
