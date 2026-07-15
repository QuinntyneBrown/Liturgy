export type InvitationStatus = 'Pending' | 'Accepted' | 'Revoked';

/** A workspace invitation as seen by a Lead managing their account. */
export interface Invitation {
  id: string;
  email: string;
  role: string;
  status: InvitationStatus;
  token: string;
  /** Relative link the client turns into a shareable URL: `{origin}/sign-up?token=…`. */
  invitePath: string;
  createdAt: string;
}

/** The public preview of a pending invitation, shown on the sign-up screen. */
export interface InvitationPreview {
  workspaceName: string;
  invitedByName: string;
  email: string;
}
