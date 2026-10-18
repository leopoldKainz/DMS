import { useState } from 'react'
import type { DocumentDto } from '../api/documents'
import { formatDateLong, formatSize } from '../lib/format'

const ROW = 'grid grid-cols-[7.5rem_1fr] gap-x-4 border-t border-linie py-3'

interface Props {
  doc: DocumentDto
  heading: 'h1' | 'h2'
  onDelete: () => void
}

export default function DocumentInfo({ doc, heading: Heading, onDelete }: Props) {
  const [confirming, setConfirming] = useState(false)

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
        <div className={ROW}>
          <dt className="text-bleistift">Größe</dt>
          <dd className="tabular-nums">{formatSize(doc.size)}</dd>
        </div>
        <div className={ROW}>
          <dt className="text-bleistift">Hinzugefügt</dt>
          <dd>{formatDateLong(doc.createdAt)}</dd>
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
