import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { deleteDocument, getDocument, type DocumentDto } from '../api/documents'
import { formatDateLong, formatSize } from '../lib/format'

const ROW = 'grid grid-cols-[7rem_1fr] gap-x-4 border-t border-rule py-4 sm:grid-cols-[10rem_1fr]'

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
      <p className="mt-12">
        <Link
          to="/"
          className="text-[13px] text-quiet hover:text-stamp focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-stamp"
        >
          Zurück zum Register
        </Link>
      </p>

      {!doc && !error ? <p className="mt-10 text-[13px] text-quiet">Wird geladen</p> : null}

      {error ? (
        <p className="mt-10 border-l-2 border-stamp py-1 pl-4 text-sm leading-relaxed">
          {error} Prüfe die Kennung in der Adresszeile.
        </p>
      ) : null}

      {doc ? (
        <>
          <h1 className="mt-6 max-w-[22ch] font-serif text-[32px] leading-[1.15] sm:text-[44px]">
            {doc.name}
          </h1>
          <p className="mt-3 font-serif text-[18px] text-stamp">.{doc.type}</p>

          <dl className="mt-12 max-w-xl border-b border-rule text-sm">
            <div className={ROW}>
              <dt className="text-quiet">Größe</dt>
              <dd className="tabular-nums">{formatSize(doc.size)}</dd>
            </div>
            <div className={ROW}>
              <dt className="text-quiet">Erstellt</dt>
              <dd>{formatDateLong(doc.createdAt)}</dd>
            </div>
            <div className={ROW}>
              <dt className="text-quiet">Kennung</dt>
              <dd className="tabular-nums">{doc.id}</dd>
            </div>
          </dl>

          <div className="mt-8 text-[13px]">
            {confirming ? (
              <>
                <button
                  type="button"
                  onClick={remove}
                  className="text-stamp underline underline-offset-4 focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-stamp"
                >
                  Endgültig löschen
                </button>
                <button
                  type="button"
                  onClick={() => setConfirming(false)}
                  className="ml-4 text-quiet hover:text-ink focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-stamp"
                >
                  Abbrechen
                </button>
              </>
            ) : (
              <button
                type="button"
                onClick={() => setConfirming(true)}
                className="text-quiet hover:text-stamp focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-stamp"
              >
                Dokument löschen
              </button>
            )}
          </div>
        </>
      ) : null}
    </>
  )
}
