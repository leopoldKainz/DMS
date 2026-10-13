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
    <div className="min-h-screen">
      <header className="border-b border-linie bg-papier">
        <div className="mx-auto flex max-w-5xl flex-wrap items-center justify-between gap-x-6 gap-y-2 px-4 py-3 sm:px-8">
          <Link to="/" className="font-condensed text-[19px] font-semibold tracking-[0.01em]">
            DMS
          </Link>
          <div className="flex items-center gap-4 text-[14px]">
            {session.username ? (
              <span className="text-bleistift">
                Angemeldet als <span className="font-medium text-graphit">{session.username}</span>
              </span>
            ) : null}
            <button
              type="button"
              onClick={logout}
              className="text-bleistift hover:text-graphit hover:underline"
            >
              Abmelden
            </button>
          </div>
        </div>
      </header>

      <main className="mx-auto max-w-5xl px-4 pb-24 sm:px-8">
        <Outlet />
      </main>
    </div>
  )
}
