import { ApiError } from './api-error'
import type { ErrorResponseDto } from '../../types/api-error.types'

function resolveBaseUrl() {
  return import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7001'
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${resolveBaseUrl()}${path}`, {
    ...init,
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
      ...(init?.headers ?? {}),
    },
  })

  const text = await res.text()
  const body = text ? (JSON.parse(text) as ErrorResponseDto | T) : null

  if (!res.ok) {
    const errorBody = body as ErrorResponseDto | null
    throw new ApiError(
      errorBody?.code ?? 'UNKNOWN_ERROR',
      errorBody?.message ?? 'Đã có lỗi xảy ra, vui lòng thử lại.',
      res.status,
      errorBody?.traceId,
      errorBody?.errors,
    )
  }

  if (res.status === 204) {
    return undefined as T
  }

  return body as T
}

export const httpClient = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, body: unknown) =>
    request<T>(path, { method: 'POST', body: JSON.stringify(body) }),
  put: <T>(path: string, body: unknown) =>
    request<T>(path, { method: 'PUT', body: JSON.stringify(body) }),
  delete: <T>(path: string) => request<T>(path, { method: 'DELETE' }),
}
