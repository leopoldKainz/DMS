import { useEffect, useState } from 'react'
import { fetchDocumentFile, type DocumentDto } from '../api/documents'
import { saveBlob, saveFile } from '../lib/download'
import { useMediaQuery } from '../lib/useMediaQuery'

type PreviewKind = 'pdf' | 'image'

const PREVIEWS: Record<string, { kind: PreviewKind; mime: string }> = {
  pdf: { kind: 'pdf', mime: 'application/pdf' },
  png: { kind: 'image', mime: 'image/png' },
  jpg: { kind: 'image', mime: 'image/jpeg' },
  jpeg: { kind: 'image', mime: 'image/jpeg' },
  gif: { kind: 'image', mime: 'image/gif' },
  webp: { kind: 'image', mime: 'image/webp' },
  svg: { kind: 'image', mime: 'image/svg+xml' },
}

const BUTTON = 'rounded-[4px] px-3 py-1.5 text-[14px] font-medium disabled:opacity-60'

interface Props {
  doc: DocumentDto
  className?: string
}

export default function DocumentViewer({ doc, className = '' }: Props) {
  const preview = PREVIEWS[doc.type.toLowerCase()]
  const coarsePointer = useMediaQuery('(pointer: coarse)')
  const inlinePdf = navigator.pdfViewerEnabled !== false && !coarsePointer
  const canPreview = preview !== undefined && (preview.kind === 'image' || inlinePdf)
  const fileName = doc.type ? `${doc.name}.${doc.type}` : doc.name

  const [url, setUrl] = useState<string | null>(null)
  const [previewError, setPreviewError] = useState<string | null>(null)
  const [downloadError, setDownloadError] = useState<string | null>(null)
  const [downloading, setDownloading] = useState(false)

  useEffect(() => {
    if (!canPreview) return

    const controller = new AbortController()
    let objectUrl: string | null = null

    fetchDocumentFile(doc.id, controller.signal)
      .then((blob) => {
        objectUrl = URL.createObjectURL(new Blob([blob], { type: preview.mime }))
        setUrl(objectUrl)
      })
      .catch((e: Error) => {
        if (!controller.signal.aborted) setPreviewError(e.message)
      })

    return () => {
      controller.abort()
      if (objectUrl) URL.revokeObjectURL(objectUrl)
    }
  }, [doc.id, canPreview, preview])

  async function download() {
    if (url) {
      saveFile(url, fileName)
      return
    }

    setDownloading(true)
    setDownloadError(null)
    try {
      saveBlob(await fetchDocumentFile(doc.id), fileName)
    } catch (e) {
      setDownloadError((e as Error).message)
    } finally {
      setDownloading(false)
    }
  }

  return (
    <section aria-label="Vorschau" className={`flex min-w-0 flex-col ${className}`}>
      <div className="flex shrink-0 flex-wrap items-center justify-between gap-x-4 gap-y-2 rounded-t-[6px] border border-linie bg-papier px-4 py-2">
        <p className="min-w-0 truncate text-[15px] font-medium">{fileName}</p>
        <div className="flex flex-wrap items-center gap-2">
          {url && preview?.kind === 'pdf' ? (
            <a
              href={url}
              target="_blank"
              rel="noopener"
              className={`${BUTTON} text-bleistift hover:bg-reiter hover:text-graphit`}
            >
              In neuem Tab öffnen
            </a>
          ) : null}
          <button
            type="button"
            onClick={download}
            disabled={downloading}
            className={`${BUTTON} bg-mappe text-white hover:brightness-110`}
          >
            {downloading ? 'Wird geladen …' : 'Herunterladen'}
          </button>
        </div>
      </div>

      {downloadError ? (
        <p role="alert" className="border-x border-linie border-l-4 border-l-typ-rot bg-papier px-4 py-2 text-[14px]">
          Download fehlgeschlagen: {downloadError}
        </p>
      ) : null}

      <div className="flex flex-col justify-center overflow-hidden rounded-b-[6px] border border-t-0 border-linie bg-reiter lg:min-h-0 lg:flex-1">
        {!canPreview ? (
          <p className="px-4 py-10 text-center text-[15px] text-bleistift">
            {preview
              ? 'Die Vorschau ist auf diesem Gerät nicht verfügbar. Lade das Dokument herunter, um es anzusehen.'
              : `Für .${doc.type}-Dateien gibt es keine Vorschau.`}
          </p>
        ) : previewError ? (
          <p role="alert" className="px-4 py-10 text-center text-[15px]">
            Die Vorschau konnte nicht geladen werden: {previewError}
          </p>
        ) : !url ? (
          <p className="px-4 py-10 text-center text-[15px] text-bleistift">Vorschau wird geladen …</p>
        ) : preview.kind === 'pdf' ? (
          <iframe src={url} title={`Vorschau von ${fileName}`} className="block h-[75vh] w-full lg:h-auto lg:flex-1" />
        ) : (
          <img src={url} alt={`Vorschau von ${fileName}`} className="mx-auto block max-h-[75vh] max-w-full object-contain lg:max-h-full" />
        )}
      </div>
    </section>
  )
}
