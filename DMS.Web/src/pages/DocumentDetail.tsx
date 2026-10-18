import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { deleteDocument, getDocument, type DocumentDto } from '../api/documents'
import DocumentInfo from '../components/DocumentInfo'
import DocumentSheet from '../components/DocumentSheet'

export default function DocumentDetail() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [doc, setDoc] = useState<DocumentDto | null>(null)
  const [error, setError] = useState<string | null>(null)

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
    <div className="max-w-5xl">
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
          <DocumentSheet doc={doc} className="mx-auto max-w-[16rem]" />
          <DocumentInfo doc={doc} heading="h1" onDelete={remove} />
        </div>
      ) : null}
    </div>
  )
}
