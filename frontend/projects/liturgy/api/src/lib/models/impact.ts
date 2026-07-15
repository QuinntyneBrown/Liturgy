/** A relationship-oriented impact metric shown on the Demonstrate screen. */
export interface ImpactMetric {
  value: string;
  unit: string | null;
  label: string;
  highlight: boolean;
}

/** A story of change, anchored to a week in the project's life. */
export interface Story {
  week: number;
  text: string;
}

/** A 5R "Rejoice" thanksgiving shown on the gratitude wall. */
export interface Gratitude {
  quote: string;
  attribution: string;
}

/** The Demonstrate/Impact surface for a project. */
export interface Impact {
  headline: string;
  metrics: ImpactMetric[];
  stories: Story[];
  gratitude: Gratitude[];
}
