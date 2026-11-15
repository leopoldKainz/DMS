import { useEffect, useState } from 'react'
import {
  getDocumentUsers,
  type DocumentDto,
  type DocumentMetadata,
  type DocumentUserDto,
  type OcrStatus,
} from '../api/documents'
import { getSession } from '../api/http'
import { formatDateLong, formatSize } from '../lib/format'

const ROW = 'grid grid-cols-[7.5rem_1fr] gap-x-4 border-t border-linie py-3'

const METADATA_FIELDS: { key: keyof DocumentMetadata; label: string; className?: string }[] = [
  { key: 'contentType', label: 'Format', className: 'break-all font-condensed text-[16px]' },
  { key: 'pageCount', label: 'Seiten', className: 'tabular-nums' },
]

const OCR_STATUS: Record<OcrStatus, { label: string; className: string }> = {
  Pending: { label: 'Ausstehend', className: 'text-bleistift' },
  Processing: { label: 'Läuft …', className: 'text-bleistift' },
  Done: { label: 'Abgeschlossen', className: 'text-mappe' },
  Failed: { label: 'Fehlgeschlagen', className: 'text-typ-rot' },
}

function displayName(user: DocumentUserDto, currentUsername: string | null | undefined): string {
  const name = user.username ?? 'Unbekannt'
  return currentUsername && user.username === currentUsername ? `${name} (du)` : name
}

interface Props {
  doc: DocumentDto
  heading: 'h1' | 'h2'
  onDelete: () => void
}

export default function DocumentInfo({ doc, heading: Heading, onDelete }: Props) {
  const [confirming, setConfirming] = useState(false)
  const [users, setUsers] = useState<DocumentUserDto[] | null>(null)
  const [usersError, setUsersError] = useState(false)
  const currentUsername = getSession()?.username

  useEffect(() => {
    const controller = new AbortController()
    getDocumentUsers(doc.id, controller.signal)
      .then(setUsers)
      .catch(() => {
        if (!controller.signal.aborted) setUsersError(true)
      })
    return () => controller.abort()
  }, [doc.id])

  const owner = users?.find((u) => u.permission === 'Owner')
  const pending = usersError ? 'Nicht verfügbar' : '…'

  return (
    <div className="min-w-0">
      <Heading
        className={`leading-tight font-semibold break-words ${
          Heading === 'h1' ? 'text-[30px] sm:text-[38px]' : 'text-[24px]'
        }`}
      >
        {doc.name}
      </Heading>

      <dl
        className={`max-w-lg border-b border-linie text-[15px] ${Heading === 'h1' ? 'mt-8' : 'mt-5'}`}
      >
        <div className={ROW}>
          <dt className="text-bleistift">Typ</dt>
          <dd className="font-condensed text-[16px]">.{doc.type}</dd>
        </div>
        {METADATA_FIELDS.map(({ key, label, className }) => {
          const value = doc.metadata?.[key]
          if (value === null || value === undefined || value === '') return null
          return (
            <div key={key} className={ROW}>
              <dt className="text-bleistift">{label}</dt>
              <dd className={className}>{value}</dd>
            </div>
          )
        })}
        <div className={ROW}>
          <dt className="text-bleistift">Größe</dt>
          <dd className="tabular-nums">{formatSize(doc.size)}</dd>
        </div>
        <div className={ROW}>
          <dt className="text-bleistift">Hinzugefügt</dt>
          <dd>{formatDateLong(doc.createdAt)}</dd>
        </div>
        {doc.ocrStatus ? (
          <div className={ROW}>
            <dt className="text-bleistift">Texterkennung</dt>
            <dd className={OCR_STATUS[doc.ocrStatus].className}>{OCR_STATUS[doc.ocrStatus].label}</dd>
          </div>
        ) : null}
        {doc.updatedOn ? (
          <div className={ROW}>
            <dt className="text-bleistift">Geändert</dt>
            <dd>{formatDateLong(doc.updatedOn)}</dd>
          </div>
        ) : null}
        <div className={ROW}>
          <dt className="text-bleistift">Besitzer</dt>
          <dd className="break-words">
            {users ? (owner ? displayName(owner, currentUsername) : 'Unbekannt') : pending}
          </dd>
        </div>
      </dl>

      <div className={`text-[15px] ${Heading === 'h1' ? 'mt-8' : 'mt-5'}`}>
        {confirming ? (
          <div className="flex flex-wrap items-center gap-x-4 gap-y-2">
            <span>Dieses Dokument endgültig löschen?</span>
            <button
              type="button"
              onClick={onDelete}
              className="rounded-[4px] bg-typ-rot px-3 py-1.5 font-medium text-white hover:brightness-95"
            >
              Löschen
            </button>
            <button
              type="button"
              onClick={() => setConfirming(false)}
              className="text-bleistift hover:text-graphit"
            >
              Behalten
            </button>
          </div>
        ) : (
          <button
            type="button"
            onClick={() => setConfirming(true)}
            className="text-bleistift hover:text-typ-rot hover:underline"
          >
            Dokument löschen
          </button>
        )}
      </div>
    </div>
  )
}
