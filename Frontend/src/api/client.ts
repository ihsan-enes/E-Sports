import type { ApiProblemDetails } from './types';

export class ApiError extends Error {
  public status: number;
  public details?: ApiProblemDetails;

  constructor(status: number, message: string, details?: ApiProblemDetails) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.details = details;
  }
}

const BASE_URL = (import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000').replace(/\/+$/, '');

export async function request<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> {
  const url = `${BASE_URL}${endpoint.startsWith('/') ? '' : '/'}${endpoint}`;

  const defaultHeaders: Record<string, string> = {
    'Accept': 'application/json'
  };

  if (options.body && !(options.body instanceof FormData)) {
    defaultHeaders['Content-Type'] = 'application/json';
  }

  const response = await fetch(url, {
    ...options,
    headers: {
      ...defaultHeaders,
      ...options.headers
    }
  });

  if (!response.ok) {
    let errorDetail: ApiProblemDetails | undefined;
    let message = `İstek başarısız oldu (${response.status})`;

    try {
      errorDetail = await response.json();
      if (errorDetail?.detail) {
        message = errorDetail.detail;
      } else if (errorDetail?.title) {
        message = errorDetail.title;
      }
    } catch {
      message = await response.text().catch(() => message);
    }

    throw new ApiError(response.status, message, errorDetail);
  }

  if (response.status === 204) {
    return undefined as unknown as T;
  }

  return response.json();
}
