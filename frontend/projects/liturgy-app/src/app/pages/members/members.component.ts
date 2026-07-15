import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { AuthStateService, Invitation, InvitationsService, Member, MembersService } from '@liturgy/api';
import { RailContextService } from '../../shell/rail-context.service';

@Component({
  selector: 'lit-members',
  imports: [],
  templateUrl: './members.component.html',
  styleUrl: './members.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MembersComponent {
  private readonly membersService = inject(MembersService);
  private readonly invitationsService = inject(InvitationsService);
  private readonly authState = inject(AuthStateService);
  private readonly rail = inject(RailContextService);

  readonly members = signal<Member[]>([]);
  readonly invitations = signal<Invitation[]>([]);
  readonly inviting = signal(false);
  readonly error = signal<string | null>(null);
  readonly lastInvite = signal<Invitation | null>(null);
  readonly copied = signal(false);

  /** The current user is a Lead if their membership role (from the members list) is Lead. */
  readonly isLead = computed(() => {
    const uid = this.authState.user()?.userId;
    return !!uid && this.members().some((m) => m.id === uid && m.role === 'Lead');
  });

  constructor() {
    this.rail.showWorkspace();
    this.loadMembers();
    this.loadInvitations();
  }

  private loadMembers(): void {
    this.membersService.list().subscribe({
      next: (m) => this.members.set(m),
      error: () => this.members.set([]),
    });
  }

  private loadInvitations(): void {
    this.invitationsService.list().subscribe({
      next: (i) => this.invitations.set(i),
      error: () => this.invitations.set([]),
    });
  }

  invite(email: string, role: string): void {
    const trimmed = email.trim();
    if (!trimmed || this.inviting()) {
      return;
    }
    this.inviting.set(true);
    this.error.set(null);
    this.copied.set(false);
    this.invitationsService.create(trimmed, role.trim() || null).subscribe({
      next: (created) => {
        this.inviting.set(false);
        this.lastInvite.set(created);
        this.loadInvitations();
      },
      error: (response: { status?: number }) => {
        this.inviting.set(false);
        this.error.set(
          response.status === 403
            ? 'Only a workspace Lead can invite members.'
            : 'Could not create the invitation — check the email address.',
        );
      },
    });
  }

  inviteUrl(invite: Invitation): string {
    return `${location.origin}${invite.invitePath}`;
  }

  copy(invite: Invitation): void {
    void navigator.clipboard?.writeText(this.inviteUrl(invite));
    this.copied.set(true);
  }

  revoke(invite: Invitation): void {
    this.invitationsService.revoke(invite.id).subscribe({ next: () => this.loadInvitations() });
  }
}
