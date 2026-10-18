import { useEffect, useRef, useState, type DragEvent } from 'react'
import { uploadDocuments } from '../api/documents'

const MAX_BYTES = 50 * 1024 * 1024

interface Props {
  onUploaded: () => void
  className?: string
}

export default function UploadField({ onUploaded, className = '' }: Props) {
  const input = useRef<HTMLInputElement>(null)
  const dragDepth = useRef(0)
  const [over, setOver] = useState(false)
  const [busy, setBusy] = useState(false)
  const [message, setMessage] = useState<string | null>(null)

  useEffect(() => {
    function block(e: globalThis.DragEvent) {
      if (e.dataTransfer?.types.includes('Files')) e.preventDefault()
    }
    window.addEventListener('dragover', block)
    window.addEventListener('drop', block)
    return () => {
      window.removeEventListener('dragover', block)
      window.removeEventListener('drop', block)
    }
  }, [])

  function validate(files: File[]): string | null {
    if (files.length === 0) return 'Keine Datei ausgewählt.'
    const empty = files.find((f) => f.size === 0)
    if (empty) return `${empty.name} ist leer und wurde nicht hochgeladen.`
    const tooBig = files.find((f) => f.size > MAX_BYTES)
    if (tooBig) return `${tooBig.name} ist größer als 50 MB und wurde nicht hochgeladen.`
    return null
  }

  function submit(files: File[]) {
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
      .catch((e: Error) => setMessage(`Hochladen fehlgeschlagen: ${e.message}`))
      .finally(() => {
        setBusy(false)
        if (input.current) input.current.value = ''
      })
  }

  function onDragEnter(e: DragEvent) {
    e.preventDefault()
    if (busy) return
    dragDepth.current += 1
    setOver(true)
  }

  function onDragLeave(e: DragEvent) {
    e.preventDefault()
    dragDepth.current = Math.max(0, dragDepth.current - 1)
    if (dragDepth.current === 0) setOver(false)
  }

  function onDrop(e: DragEvent) {
    e.preventDefault()
    dragDepth.current = 0
    setOver(false)
    if (busy) return
    submit(Array.from(e.dataTransfer.files))
  }

  let prompt: string
  if (busy) prompt = 'Wird hochgeladen …'
  else if (over) prompt = 'Loslassen zum Hochladen'
  else prompt = 'Dateien hier ablegen'

  return (
    <div
      onDragEnter={onDragEnter}
      onDragOver={(e) => e.preventDefault()}
      onDragLeave={onDragLeave}
      onDrop={onDrop}
      className={`rounded-[6px] border border-dashed px-4 py-6 text-center transition-colors motion-reduce:transition-none ${
        over ? 'border-mappe bg-mappe/8' : 'border-bleistift/40'
      } ${className}`}
    >
      <p className={`text-[15px] ${over ? 'font-medium text-mappe' : 'text-bleistift'}`}>
        {prompt}
        {!busy && !over ? (
          <>
            {' oder '}
            <label className="cursor-pointer font-medium text-mappe underline underline-offset-4 focus-within:outline-2 focus-within:outline-offset-3 focus-within:outline-mappe">
              auswählen
              <input
                ref={input}
                type="file"
                multiple
                onChange={(e) => submit(Array.from(e.target.files ?? []))}
                className="sr-only"
              />
            </label>
          </>
        ) : null}
      </p>
      <p className="mt-1 text-[13px] text-bleistift">Bis zu 50 MB pro Datei</p>
      {message ? (
        <p role="status" className="mt-3 text-[14px] text-graphit">
          {message}
        </p>
      ) : null}
    </div>
  )
}
