import { useRef, useState } from 'react'
import { uploadDocuments } from '../api/documents'

const MAX_BYTES = 50 * 1024 * 1024

interface Props {
  userId: number | undefined
  onUploaded: () => void
}

export default function UploadField({ userId, onUploaded }: Props) {
  const input = useRef<HTMLInputElement>(null)
  const [busy, setBusy] = useState(false)
  const [message, setMessage] = useState<string | null>(null)

  function validate(files: File[]): string | null {
    if (files.length === 0) return 'Keine Datei ausgewählt.'
    const empty = files.find((f) => f.size === 0)
    if (empty) return `${empty.name} ist leer.`
    const tooBig = files.find((f) => f.size > MAX_BYTES)
    if (tooBig) return `${tooBig.name} überschreitet 50 MB.`
    return null
  }

  function submit(fileList: FileList | null) {
    if (userId === undefined) return
    const files = Array.from(fileList ?? [])
    const problem = validate(files)
    if (problem) {
      setMessage(problem)
      return
    }

    setBusy(true)
    setMessage(null)
    uploadDocuments(files, userId)
      .then((created) => {
        setMessage(
          created.length === 1
            ? `${created[0].name} hochgeladen.`
            : `${created.length} Dateien hochgeladen.`,
        )
        onUploaded()
      })
      .catch((e: Error) => setMessage(e.message))
      .finally(() => {
        setBusy(false)
        if (input.current) input.current.value = ''
      })
  }

  if (userId === undefined) {
    return (
      <p className="text-[13px] text-quiet">Wähle oben einen Benutzer, um Dateien hochzuladen.</p>
    )
  }

  return (
    <div className="text-[13px]">
      <label className="cursor-pointer border-b border-stamp pb-0.5 text-stamp focus-within:outline-2 focus-within:outline-offset-4 focus-within:outline-stamp">
        {busy ? 'Wird hochgeladen' : 'Dateien hochladen'}
        <input
          ref={input}
          type="file"
          multiple
          disabled={busy}
          onChange={(e) => submit(e.target.files)}
          className="sr-only"
        />
      </label>
      {message ? <p className="mt-2 text-quiet">{message}</p> : null}
    </div>
  )
}
