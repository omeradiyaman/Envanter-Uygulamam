// An explicitly empty VITE_API_BASE_URL produces relative /api calls,
// which the Nginx container proxies to the backend (Docker setup).
const apiBaseUrl = (
  import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7001'
).replace(/\/$/, '')

interface ProblemDetails {
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

export class ApiError extends Error {
  public readonly status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

async function request<T>(
  path: string,
  options: RequestInit,
  signal?: AbortSignal,
): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...options,
    credentials: 'include',
    headers: {
      Accept: 'application/json',
      ...(options.body ? { 'Content-Type': 'application/json' } : {}),
      ...options.headers,
    },
    signal,
  })

  if (!response.ok) {
    if (response.status === 401 && path !== '/api/auth/login') window.dispatchEvent(new Event('auth:unauthorized'))
    await throwApiError(response)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

async function throwApiError(response: Response): Promise<never> {
  const problem = (await response.json().catch(() => null)) as ProblemDetails | null
  const validationMessage = problem?.errors
    ? Object.values(problem.errors).flat().join(' ')
    : null
  throw new ApiError(
    validationMessage ?? problem?.detail ?? problem?.title ?? `API isteği başarısız oldu (${response.status}).`,
    response.status,
  )
}

export function get<T>(path: string, signal?: AbortSignal): Promise<T> {
  return request<T>(path, { method: 'GET' }, signal)
}

export function post<TBody, TResponse>(
  path: string,
  body: TBody,
  signal?: AbortSignal,
): Promise<TResponse> {
  return request<TResponse>(
    path,
    { method: 'POST', body: JSON.stringify(body) },
    signal,
  )
}

export function put<TBody, TResponse>(
  path: string,
  body: TBody,
  signal?: AbortSignal,
): Promise<TResponse> {
  return request<TResponse>(
    path,
    { method: 'PUT', body: JSON.stringify(body) },
    signal,
  )
}

export function del(path: string, signal?: AbortSignal): Promise<void> {
  return request<void>(path, { method: 'DELETE' }, signal)
}

export async function getBlob(path: string, signal?: AbortSignal): Promise<Blob> {
  const response = await fetch(`${apiBaseUrl}${path}`, { credentials: 'include', headers: { Accept: '*/*' }, signal })
  if (!response.ok) await throwApiError(response)
  return response.blob()
}

export async function postForm<T>(path: string, form: FormData, signal?: AbortSignal): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    method: 'POST',
    credentials: 'include',
    headers: { Accept: 'application/json' },
    body: form,
    signal,
  })
  if (!response.ok) await throwApiError(response)
  return response.json() as Promise<T>
}
