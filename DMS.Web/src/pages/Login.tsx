import { useState, type FormEvent } from 'react'
import { Link, Navigate, useLocation, useNavigate } from 'react-router'
import { getSession, saveSession } from '../api/http'
import { login } from '../api/users'
import AuthLayout from '../components/AuthLayout'
import TextField from '../components/TextField'

interface Errors {
  usernameOrEmail?: string
  password?: string
}

function validate(usernameOrEmail: string, password: string): Errors {
  const errors: Errors = {}
  if (!usernameOrEmail.trim()) errors.usernameOrEmail = 'Benutzername oder E-Mail fehlt.'
  else if (usernameOrEmail.trim().length > 50) errors.usernameOrEmail = 'Höchstens 50 Zeichen.'
  if (!password) errors.password = 'Passwort fehlt.'
  return errors
}

export default function Login() {
  const navigate = useNavigate()
  const location = useLocation()
  const registered = (location.state as { registered?: string } | null)?.registered

  const [usernameOrEmail, setUsernameOrEmail] = useState(registered ?? '')
  const [password, setPassword] = useState('')
  const [errors, setErrors] = useState<Errors>({})
  const [failure, setFailure] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  if (getSession()) return <Navigate to="/" replace />

  function submit(e: FormEvent) {
    e.preventDefault()
    const found = validate(usernameOrEmail, password)
    setErrors(found)
    setFailure(null)
    if (Object.keys(found).length > 0) return

    setBusy(true)
    login(usernameOrEmail.trim(), password)
      .then((res) => {
        saveSession({ token: res.token, expiresAt: res.expiresAt, username: res.user.username })
        navigate('/', { replace: true })
      })
      .catch((err: Error) => setFailure(err.message))
      .finally(() => setBusy(false))
  }

  return (
    <AuthLayout
      title="Anmelden"
      intro="Melde dich an, um deine Dokumente zu sehen und neue hochzuladen."
      footer={
        <>
          Noch kein Konto?{' '}
          <Link to="/register" className="font-medium text-mappe underline underline-offset-4">
            Registrieren
          </Link>
        </>
      }
    >
      {registered ? (
        <p className="mb-6 border-l-4 border-mappe py-1 pl-3 text-[15px]">
          Konto angelegt. Du kannst dich jetzt anmelden.
        </p>
      ) : null}

      <form noValidate onSubmit={submit} className="space-y-5">
        <TextField
          label="Benutzername oder E-Mail"
          name="usernameOrEmail"
          autoComplete="username"
          autoFocus={!registered}
          value={usernameOrEmail}
          onChange={(e) => setUsernameOrEmail(e.target.value)}
          error={errors.usernameOrEmail}
        />
        <TextField
          label="Passwort"
          name="password"
          type="password"
          autoComplete="current-password"
          autoFocus={Boolean(registered)}
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          error={errors.password}
        />

        {failure ? (
          <p role="alert" className="border-l-4 border-typ-rot py-1 pl-3 text-[15px]">
            {failure}
          </p>
        ) : null}

        <button
          type="submit"
          disabled={busy}
          className="w-full rounded-[4px] bg-mappe py-2.5 text-[15px] font-medium text-white hover:brightness-110 disabled:opacity-60"
        >
          {busy ? 'Wird angemeldet' : 'Anmelden'}
        </button>
      </form>
    </AuthLayout>
  )
}
