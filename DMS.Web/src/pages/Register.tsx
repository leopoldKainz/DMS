import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router'
import { register } from '../api/users'
import AuthLayout from '../components/AuthLayout'
import TextField from '../components/TextField'

interface Form {
  username: string
  email: string
  password: string
  confirm: string
}

type Errors = Partial<Record<keyof Form, string>>

const EMAIL = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

function validate(form: Form): Errors {
  const errors: Errors = {}
  const username = form.username.trim()
  const email = form.email.trim()

  if (!username) errors.username = 'Benutzername fehlt.'
  else if (username.length > 20) errors.username = 'Höchstens 20 Zeichen.'

  if (!email) errors.email = 'E-Mail fehlt.'
  else if (!EMAIL.test(email)) errors.email = 'Keine gültige E-Mail-Adresse.'
  else if (email.length > 50) errors.email = 'Höchstens 50 Zeichen.'

  if (!form.password) errors.password = 'Passwort fehlt.'
  else if (form.password.length < 6) errors.password = 'Mindestens 6 Zeichen.'

  if (form.confirm !== form.password) errors.confirm = 'Passwörter stimmen nicht überein.'

  return errors
}

export default function Register() {
  const navigate = useNavigate()
  const [form, setForm] = useState<Form>({ username: '', email: '', password: '', confirm: '' })
  const [errors, setErrors] = useState<Errors>({})
  const [failure, setFailure] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  function update(field: keyof Form, value: string) {
    setForm((current) => ({ ...current, [field]: value }))
  }

  function submit(e: FormEvent) {
    e.preventDefault()
    const found = validate(form)
    setErrors(found)
    setFailure(null)
    if (Object.keys(found).length > 0) return

    setBusy(true)
    register(form.username.trim(), form.email.trim(), form.password)
      .then((user) =>
        navigate('/login', { state: { registered: user.username ?? form.username.trim() } }),
      )
      .catch((err: Error) => setFailure(err.message))
      .finally(() => setBusy(false))
  }

  return (
    <AuthLayout
      title="Registrieren"
      intro="Lege ein Konto an. Danach kannst du dich anmelden und Dokumente verwalten."
      footer={
        <>
          Schon registriert?{' '}
          <Link to="/login" className="font-medium text-mappe underline underline-offset-4">
            Anmelden
          </Link>
        </>
      }
    >
      <form noValidate onSubmit={submit} className="space-y-5">
        <TextField
          label="Benutzername"
          name="username"
          autoComplete="username"
          autoFocus
          value={form.username}
          onChange={(e) => update('username', e.target.value)}
          error={errors.username}
        />
        <TextField
          label="E-Mail"
          name="email"
          type="email"
          autoComplete="email"
          value={form.email}
          onChange={(e) => update('email', e.target.value)}
          error={errors.email}
        />
        <TextField
          label="Passwort"
          name="password"
          type="password"
          autoComplete="new-password"
          value={form.password}
          onChange={(e) => update('password', e.target.value)}
          error={errors.password}
        />
        <TextField
          label="Passwort wiederholen"
          name="confirm"
          type="password"
          autoComplete="new-password"
          value={form.confirm}
          onChange={(e) => update('confirm', e.target.value)}
          error={errors.confirm}
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
          {busy ? 'Wird angelegt' : 'Konto anlegen'}
        </button>
      </form>
    </AuthLayout>
  )
}
