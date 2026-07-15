import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthStateService } from '@liturgy/api';

@Component({
  selector: 'lit-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShellComponent {
  private readonly authState = inject(AuthStateService);
  private readonly router = inject(Router);

  readonly user = this.authState.user;

  signOut(): void {
    this.authState.clear();
    void this.router.navigate(['/sign-in']);
  }
}
