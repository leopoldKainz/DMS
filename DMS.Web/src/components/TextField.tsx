import type { InputHTMLAttributes } from 'react'

interface Props extends InputHTMLAttributes<HTMLInputElement> {
  label: string
  error?: string | null
}

export default function TextField({ label, error, id, ...input }: Props) {
  const fieldId = id ?? input.name
  const errorId = error ? `${fieldId}-error` : undefined

  return (
    <div>
      <label htmlFor={fieldId} className="text-[14px] font-medium">
        {label}
      </label>
      <input
        id={fieldId}
        aria-invalid={error ? true : undefined}
        aria-describedby={errorId}
        {...input}
        className={`mt-1.5 block w-full rounded-[4px] border bg-white px-3 py-2 text-[16px] placeholder:text-bleistift/70 focus:outline-none ${
          error ? 'border-typ-rot' : 'border-linie focus:border-mappe'
        }`}
      />
      {error ? (
        <p id={errorId} className="mt-1.5 text-[14px] text-typ-rot">
          {error}
        </p>
      ) : null}
    </div>
  )
}
