/**
 * Origin of the Liturgy API. In dev the backend runs on 5099; the Playwright suite
 * intercepts `** /api/**` regardless of origin, so this value is used in every mode.
 */
export const API_ORIGIN = 'http://localhost:5099';
