const apiBaseUrl = (
  import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7001'
).replace(/\/$/, '')

export async function get<T>(
  path: string,
  signal?: AbortSignal,
): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    method: 'GET',
    headers: {
      Accept: 'application/json',
    },
    signal,
  })

  if (!response.ok) {
    throw new Error(`API isteği başarısız oldu (${response.status}).`)
  }

  return (await response.json()) as T
}
