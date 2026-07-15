import { Injectable, computed, signal } from '@angular/core';
import { AuthResult } from '../models/auth-result';

interface StoredSession {
  token: string;
  user: AuthResult;
}

const STORAGE_KEY = 'liturgy.auth';

@Injectable({ providedIn: 'root' })
export class AuthStateService {
  private readonly _user = signal<AuthResult | null>(null);
  private readonly _token = signal<string | null>(null);

  readonly user = this._user.asReadonly();
  readonly token = this._token.asReadonly();
  readonly isAuthenticated = computed(() => this._token() !== null);

  constructor() {
    this.restore();
  }

  setSession(result: AuthResult): void {
    this._user.set(result);
    this._token.set(result.accessToken);
    this.persist({ token: result.accessToken, user: result });
  }

  clear(): void {
    this._user.set(null);
    this._token.set(null);
    this.remove();
  }

  private restore(): void {
    const raw = this.readStorage();
    if (!raw) {
      return;
    }

    try {
      const session = JSON.parse(raw) as StoredSession;
      this._user.set(session.user);
      this._token.set(session.token);
    } catch {
      this.remove();
    }
  }

  private persist(session: StoredSession): void {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
    } catch {
      // Storage may be unavailable (private mode / SSR); session stays in memory.
    }
  }

  private readStorage(): string | null {
    try {
      return localStorage.getItem(STORAGE_KEY);
    } catch {
      return null;
    }
  }

  private remove(): void {
    try {
      localStorage.removeItem(STORAGE_KEY);
    } catch {
      // ignore
    }
  }
}
