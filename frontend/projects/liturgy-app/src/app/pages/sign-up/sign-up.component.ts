import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService, AuthStateService } from '@liturgy/api';

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

  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    firstName: ['', [Validators.required]],
    lastName: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(12)]],
  });

  submit(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.error.set(null);
    const { firstName, lastName, email, password } = this.form.getRawValue();

    this.auth.register(email, firstName, lastName, password).subscribe({
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
