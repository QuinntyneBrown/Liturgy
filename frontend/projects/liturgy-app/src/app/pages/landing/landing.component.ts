import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DialComponent, RListComponent } from '@liturgy/components';

@Component({
  selector: 'lit-landing',
  imports: [RouterLink, DialComponent, RListComponent],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LandingComponent {
  protected readonly currentYear = new Date().getFullYear();
}
