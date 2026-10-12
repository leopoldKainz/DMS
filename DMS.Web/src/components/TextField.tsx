import type { InputHTMLAttributes } from 'react'

interface Props extends InputHTMLAttributes<HTMLInputElement> {
  label: string
  error?: string | null
}

export default function TextField({ label, error, id, ...input }: Props) {
  const fieldId = id ?? input.name
  const errorId = error ? `${fieldId}-error` : undefined

  return (
    <div className="text-[13px]">
      <label htmlFor={fieldId} className="text-quiet">
        {label}
      </label>
      <input
        id={fieldId}
        aria-invalid={error ? true : undefined}
        aria-describedby={errorId}
        {...input}
        className={`mt-1 block w-full border-b bg-transparent pb-1.5 text-[15px] text-ink placeholder:text-quiet/60 focus:outline-none ${
          error ? 'border-stamp' : 'border-rule focus:border-stamp'
        }`}
      />
      {error ? (
        <p id={errorId} className="mt-1.5 text-stamp">
          {error}
        </p>
      ) : null}
    </div>
  )
}
