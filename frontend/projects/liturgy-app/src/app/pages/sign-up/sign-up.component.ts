import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService, AuthStateService, InvitationPreview, InvitationsService } from '@liturgy/api';

@Component({
  selector: 'lit-sign-up',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './sign-up.component.html',
  styleUrl: './sign-up.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SignUpComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly authState = inject(AuthStateService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly invitations = inject(InvitationsService);

  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);
  readonly inviteToken = signal<string | null>(null);
  readonly invitePreview = signal<InvitationPreview | null>(null);

  readonly form = this.fb.nonNullable.group({
    firstName: ['', [Validators.required]],
    lastName: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(12)]],
  });

  constructor() {
    const token = this.route.snapshot.queryParamMap.get('token');
    if (token) {
      this.inviteToken.set(token);
      this.invitations.getByToken(token).subscribe({
        next: (preview) => {
          this.invitePreview.set(preview);
          this.form.controls.email.setValue(preview.email);
        },
        // An invalid/expired token just falls back to an ordinary sign-up.
        error: () => this.inviteToken.set(null),
      });
    }
  }

  submit(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.error.set(null);
    const { firstName, lastName, email, password } = this.form.getRawValue();

    this.auth.register(email, firstName, lastName, password, this.inviteToken()).subscribe({
      next: (result) => {
        this.authState.setSession(result);
        void this.router.navigate(['/projects']);
      },
      error: (response: { status?: number }) => {
        this.error.set(
          response.status === 409
            ? 'That email is already registered. Try signing in.'
            : 'Password must be at least 12 characters with upper, lower, digit, and symbol.',
        );
        this.submitting.set(false);
      },
    });
  }
}
