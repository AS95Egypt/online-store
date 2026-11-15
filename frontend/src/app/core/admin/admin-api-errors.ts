import { HttpErrorResponse } from '@angular/common/http';

export interface AdminApiError {
  message: string;
  fields: Record<string, string>;
}

export function readAdminApiError(error: unknown, fallback: string): AdminApiError {
  if (!(error instanceof HttpErrorResponse) || typeof error.error !== 'object' || error.error === null) {
    return { message: fallback, fields: {} };
  }

  const body = error.error as {
    error?: { message?: unknown; details?: unknown };
    message?: unknown;
    title?: unknown;
    errors?: unknown;
  };
  const apiError = body.error;
  const message = [apiError?.message, body.message, body.title]
    .find((value): value is string => typeof value === 'string' && value.trim().length > 0) ?? fallback;
  const details = apiError?.details ?? body.errors;
  const fields: Record<string, string> = {};
  if (typeof details === 'object' && details !== null) {
    for (const [key, value] of Object.entries(details)) {
      if (Array.isArray(value)) {
        const text = value.find((item): item is string => typeof item === 'string');
        if (text) {
          fields[key.toLowerCase()] = text;
        }
      } else if (typeof value === 'string') {
        fields[key.toLowerCase()] = value;
      }
    }
  }

  return { message, fields };
}
