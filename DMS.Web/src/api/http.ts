const BASE = '/api'

export class ApiError extends Error {
  readonly status: number

  constructor(status: number, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

async function readError(res: Response): Promise<string> {
  const text = await res.text()
  if (!text) return `HTTP ${res.status}`

  try {
    const body: unknown = JSON.parse(text)
    if (body && typeof body === 'object') {
      const problem = body as Record<string, unknown>
      if (problem.errors && typeof problem.errors === 'object') {
        return Object.values(problem.errors as Record<string, string[]>)
          .flat()
          .join(' ')
      }
      if (typeof problem.detail === 'string') return problem.detail
      if (typeof problem.title === 'string') return problem.title
    }
  } catch {
    return text
  }

  return text
}

export async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(BASE + path, init)
  if (!res.ok) throw new ApiError(res.status, await readError(res))
  if (res.status === 204 || res.headers.get('content-length') === '0') {
    return undefined as T
  }
  return (await res.json()) as T
}

export function query(params: Record<string, string | number | undefined>): string {
  const search = new URLSearchParams()
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== '') search.set(key, String(value))
  }
  const s = search.toString()
  return s ? `?${s}` : ''
}
