import { useRef, useState } from 'react'
import { uploadDocuments } from '../api/documents'

const MAX_BYTES = 50 * 1024 * 1024

interface Props {
  onUploaded: () => void
}

export default function UploadField({ onUploaded }: Props) {
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
    const files = Array.from(fileList ?? [])
    const problem = validate(files)
    if (problem) {
      setMessage(problem)
      return
    }

    setBusy(true)
    setMessage(null)
    uploadDocuments(files)
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
