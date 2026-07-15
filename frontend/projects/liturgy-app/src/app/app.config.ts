import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection,
} from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import {
  API_BASE_URL,
  BoardRealtime,
  SignalrBoardRealtimeService,
  authInterceptor,
} from '@liturgy/api';

import { routes } from './app.routes';
import { API_ORIGIN } from './api-origin';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withInterceptors([authInterceptor])),
    { provide: API_BASE_URL, useValue: API_ORIGIN },
    { provide: BoardRealtime, useClass: SignalrBoardRealtimeService },
  ],
};
