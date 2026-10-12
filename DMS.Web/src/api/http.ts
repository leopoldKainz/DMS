const BASE = '/api'
const SESSION_KEY = 'dms.session'

export interface Session {
  token: string
  expiresAt: string
  username: string | null
}

export function getSession(): Session | null {
  try {
    const raw = localStorage.getItem(SESSION_KEY)
    if (!raw) return null
    const session = JSON.parse(raw) as Session
    if (Date.parse(session.expiresAt) <= Date.now()) {
      localStorage.removeItem(SESSION_KEY)
      return null
    }
    return session
  } catch {
    return null
  }
}

export function saveSession(session: Session) {
  try {
    localStorage.setItem(SESSION_KEY, JSON.stringify(session))
  } catch {
    return
  }
}

export function clearSession() {
  try {
    localStorage.removeItem(SESSION_KEY)
  } catch {
    return
  }
}

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
  const headers = new Headers(init?.headers)
  const session = getSession()
  if (session) headers.set('Authorization', `Bearer ${session.token}`)

  const res = await fetch(BASE + path, { ...init, headers })

  if (res.status === 401 && session) {
    clearSession()
    window.location.assign('/login')
  }
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
