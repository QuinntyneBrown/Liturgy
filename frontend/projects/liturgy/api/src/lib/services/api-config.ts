import { InjectionToken } from '@angular/core';

/** Base URL of the Liturgy API, e.g. `http://localhost:5099`. Provided by the app. */
export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL');
