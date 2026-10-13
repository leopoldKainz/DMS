import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { deleteDocument, getDocument, type DocumentDto } from '../api/documents'
import { formatDateLong, formatSize } from '../lib/format'
import { typeColor, typeInk } from '../lib/fileTypes'

const ROW = 'grid grid-cols-[7.5rem_1fr] gap-x-4 border-t border-linie py-3'

// The document drawn as an A4 sheet (1:√2) with its type tab sticking out at the top.
function Sheet({ doc }: { doc: DocumentDto }) {
  const color = typeColor(doc.type)
  return (
    <div className="relative mx-auto w-full max-w-[16rem] pt-7" aria-hidden>
      <span
        className="absolute top-0 right-6 h-8 rounded-t-[5px] px-3 font-condensed text-[15px] leading-7 font-semibold"
        style={{ backgroundColor: color, color: typeInk(doc.type) }}
      >
        .{doc.type}
      </span>
      <div
        className="relative flex aspect-[1/1.4142] flex-col justify-between border border-linie bg-papier p-5"
        style={{ boxShadow: `inset 0 3px 0 ${color}` }}
      >
        <div className="space-y-2.5 pt-3">
          {[92, 100, 84, 96, 61].map((w, i) => (
            <span key={i} className="block h-[3px] bg-linie" style={{ width: `${w}%` }} />
          ))}
        </div>
        <p className="font-condensed text-[15px] leading-snug break-words text-bleistift">
          {doc.name}.{doc.type}
        </p>
      </div>
    </div>
  )
}

export default function DocumentDetail() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [doc, setDoc] = useState<DocumentDto | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [confirming, setConfirming] = useState(false)

  useEffect(() => {
    getDocument(id!)
      .then(setDoc)
      .catch((e: Error) => setError(e.message))
  }, [id])

  function remove() {
    if (!doc) return
    deleteDocument(doc.id)
      .then(() => navigate('/'))
      .catch((e: Error) => setError(e.message))
  }

  return (
    <>
      <p className="mt-8">
        <Link to="/" className="text-[14px] text-bleistift hover:text-graphit hover:underline">
          Zurück zu den Dokumenten
        </Link>
      </p>

      {!doc && !error ? <p className="mt-10 text-[15px] text-bleistift">Wird geladen …</p> : null}

      {error ? (
        <p role="alert" className="mt-8 border-l-4 border-typ-rot bg-papier px-4 py-3 text-[15px]">
          {error} Prüfe die Adresse oder geh zurück zur Übersicht.
        </p>
      ) : null}

      {doc ? (
        <div className="mt-8 grid gap-10 md:grid-cols-[16rem_1fr] md:gap-14">
          <Sheet doc={doc} />

          <div className="min-w-0">
            <h1 className="text-[30px] leading-tight font-semibold break-words sm:text-[38px]">
              {doc.name}
            </h1>

            <dl className="mt-8 max-w-lg border-b border-linie text-[15px]">
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

            <div className="mt-8 text-[15px]">
              {confirming ? (
                <div className="flex flex-wrap items-center gap-x-4 gap-y-2">
                  <span>Dieses Dokument endgültig löschen?</span>
                  <button
                    type="button"
                    onClick={remove}
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
        </div>
      ) : null}
    </>
  )
}
