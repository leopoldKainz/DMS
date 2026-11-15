import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { deleteDocument, getDocument, type DocumentDto } from '../api/documents'
import DocumentInfo from '../components/DocumentInfo'
import DocumentViewer from '../components/DocumentViewer'

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
    <div className="flex flex-col py-6 lg:h-full">
      <p className="shrink-0">
        <Link to="/" className="text-[14px] text-bleistift hover:text-graphit hover:underline">
          Zurück zu den Dokumenten
        </Link>
      </p>

      {!doc && !error ? <p className="mt-10 text-[15px] text-bleistift">Wird geladen …</p> : null}

      {error ? (
        <p role="alert" className="mt-6 shrink-0 border-l-4 border-typ-rot bg-papier px-4 py-3 text-[15px]">
          {error} Prüfe die Adresse oder geh zurück zur Übersicht.
        </p>
      ) : null}

      {doc ? (
        <div className="mt-5 grid gap-8 lg:min-h-0 lg:flex-1 lg:grid-cols-[minmax(0,1fr)_20rem] xl:grid-cols-[minmax(0,1fr)_24rem]">
          <DocumentViewer key={doc.id} doc={doc} className="lg:min-h-0" />
          <aside aria-label="Metadaten" className="lg:min-h-0 lg:overflow-y-auto">
            <DocumentInfo key={doc.id} doc={doc} heading="h1" onDelete={remove} />
          </aside>
        </div>
      ) : null}
    </div>
  )
}
