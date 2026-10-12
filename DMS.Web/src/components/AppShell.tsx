import { useEffect, useState } from 'react'
import { Link, Outlet } from 'react-router'
import { listUsers, type UserDto } from '../api/users'

const STORAGE_KEY = 'dms.userId'

export interface ShellContext {
  userId: number | undefined
  users: UserDto[]
}

function readStoredUserId(): number | undefined {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    return raw ? Number(raw) : undefined
  } catch {
    return undefined
  }
}

function storeUserId(id: number | undefined) {
  try {
    if (id === undefined) localStorage.removeItem(STORAGE_KEY)
    else localStorage.setItem(STORAGE_KEY, String(id))
  } catch {
    return
  }
}

export default function AppShell() {
  const [users, setUsers] = useState<UserDto[]>([])
  const [userId, setUserId] = useState<number | undefined>(readStoredUserId)

  useEffect(() => {
    listUsers()
      .then(setUsers)
      .catch(() => setUsers([]))
  }, [])

  function choose(value: string) {
    const next = value === '' ? undefined : Number(value)
    setUserId(next)
    storeUserId(next)
  }

  return (
    <div className="min-h-screen bg-paper text-ink">
      <div className="mx-auto max-w-5xl px-5 pb-24 pt-8 sm:px-8">
        <div className="flex flex-wrap items-center gap-x-5 gap-y-3">
          <Link
            to="/"
            className="shrink-0 font-serif text-[15px] hover:text-stamp focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-stamp"
          >
            Dokumentenverwaltung
          </Link>
          <span className="hidden h-px flex-1 bg-rule sm:block" />
          <label className="flex items-center gap-2 text-[13px] text-quiet">
            Benutzer
            <select
              value={userId ?? ''}
              onChange={(e) => choose(e.target.value)}
              className="border-b border-rule bg-transparent py-0.5 text-ink focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-stamp"
            >
              <option value="">Alle</option>
              {users.map((user) => (
                <option key={user.id} value={user.id}>
                  {user.username ?? `Nr. ${user.id}`}
                </option>
              ))}
            </select>
          </label>
        </div>

        <Outlet context={{ userId, users } satisfies ShellContext} />
      </div>
    </div>
  )
}
