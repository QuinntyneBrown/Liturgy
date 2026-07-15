import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter, map } from 'rxjs';
import { AuthStateService, phaseOrdinal } from '@liturgy/api';
import { RhythmRailComponent } from '@liturgy/components';
import { RailContextService } from './rail-context.service';

@Component({
  selector: 'lit-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, RhythmRailComponent],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShellComponent {
  private readonly authState = inject(AuthStateService);
  private readonly router = inject(Router);
  readonly rail = inject(RailContextService);

  readonly user = this.authState.user;
  readonly mode = this.rail.mode;
  readonly journey = this.rail.journey;
  readonly fiveRFilled = this.rail.fiveRFilled;

  private readonly url = toSignal(
    this.router.events.pipe(
      filter((e): e is NavigationEnd => e instanceof NavigationEnd),
      map(() => this.router.url),
    ),
    { initialValue: this.router.url },
  );

  readonly screenLabel = computed(() => {
    const segments = this.url().split('?')[0].split('/').filter(Boolean);
    switch (segments[0]) {
      case 'dashboard':
        return 'Dashboard';
      case 'projects':
        return segments.length > 1 ? '4D Journey' : 'Projects';
      case 'discern':
        return 'Discern';
      case 'board':
        return 'Develop board';
      case 'loop':
        return '5R loop';
      case 'demonstrate':
        return 'Demonstrate';
      default:
        return 'Workspace';
    }
  });

  readonly phasePill = computed(() => {
    const journey = this.journey();
    if (this.mode() !== 'project' || !journey) {
      return null;
    }
    return `${phaseOrdinal(journey.currentPhase)} ${journey.currentPhase}`;
  });

  signOut(): void {
    this.authState.clear();
    void this.router.navigate(['/sign-in']);
  }
}
