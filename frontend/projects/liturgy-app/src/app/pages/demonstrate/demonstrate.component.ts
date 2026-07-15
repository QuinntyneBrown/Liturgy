import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Impact, ImpactService } from '@liturgy/api';
import { RailContextService } from '../../shell/rail-context.service';

@Component({
  selector: 'lit-demonstrate',
  imports: [RouterLink],
  templateUrl: './demonstrate.component.html',
  styleUrl: './demonstrate.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DemonstrateComponent {
  private readonly impactService = inject(ImpactService);
  private readonly rail = inject(RailContextService);

  readonly projectId = input.required<string>();
  readonly impact = signal<Impact | null>(null);

  constructor() {
    effect(() => {
      const id = this.projectId();
      this.rail.showProject(id);
      this.impactService.get(id).subscribe((impact) => this.impact.set(impact));
    });
  }
}
