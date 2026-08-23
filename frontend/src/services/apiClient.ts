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
    headers: {
      Accept: 'application/json',
      ...(options.body ? { 'Content-Type': 'application/json' } : {}),
      ...options.headers,
    },
    signal,
  })

  if (!response.ok) {
    const problem = (await response.json().catch(() => null)) as ProblemDetails | null
    const validationMessage = problem?.errors
      ? Object.values(problem.errors).flat().join(' ')
      : null

    throw new ApiError(
      validationMessage
        ?? problem?.detail
        ?? problem?.title
        ?? `API isteği başarısız oldu (${response.status}).`,
      response.status,
    )
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
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
