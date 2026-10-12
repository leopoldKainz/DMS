import { Link, Navigate, Outlet, useNavigate } from 'react-router'
import { clearSession, getSession } from '../api/http'

export default function AppShell() {
  const navigate = useNavigate()
  const session = getSession()

  if (!session) return <Navigate to="/login" replace />

  function logout() {
    clearSession()
    navigate('/login', { replace: true })
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
          <div className="flex items-center gap-4 text-[13px] text-quiet">
            {session.username ? <span className="text-ink">{session.username}</span> : null}
            <button
              type="button"
              onClick={logout}
              className="hover:text-stamp focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-stamp"
            >
              Abmelden
            </button>
          </div>
        </div>

        <Outlet />
      </div>
    </div>
  )
}
