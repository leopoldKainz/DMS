import { useEffect, useMemo, useState } from 'react'
import {
  deleteDocument,
  listDocuments,
  searchDocuments,
  type DocumentDto,
} from '../api/documents'
import DocumentsTable, { type Sort, type SortKey } from '../components/DocumentsTable'
import RegisterTabs, { type TypeCount } from '../components/RegisterTabs'
import UploadField from '../components/UploadField'

function compare(a: DocumentDto, b: DocumentDto, sort: Sort): number {
  const dir = sort.dir === 'asc' ? 1 : -1
  if (sort.key === 'size') return (a.size - b.size) * dir
  if (sort.key === 'createdAt') {
    return (Date.parse(a.createdAt) - Date.parse(b.createdAt)) * dir
  }
  return a[sort.key].localeCompare(b[sort.key], 'de') * dir
}

export default function Dashboard() {
  const [documents, setDocuments] = useState<DocumentDto[] | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [term, setTerm] = useState('')
  const [debounced, setDebounced] = useState('')
  const [typeFilter, setTypeFilter] = useState<string | null>(null)
  const [sort, setSort] = useState<Sort>({ key: 'createdAt', dir: 'desc' })
  const [reloads, setReloads] = useState(0)

  useEffect(() => {
    const timer = setTimeout(() => setDebounced(term.trim()), 250)
    return () => clearTimeout(timer)
  }, [term])

  useEffect(() => {
    let cancelled = false
    const pending = debounced ? searchDocuments(debounced) : listDocuments()

    pending
      .then((docs) => {
        if (cancelled) return
        setDocuments(docs)
        setError(null)
      })
      .catch((e: Error) => {
        if (cancelled) return
        setDocuments(null)
        setError(e.message)
      })

    return () => {
      cancelled = true
    }
  }, [debounced, reloads])

  const counts = useMemo<TypeCount[]>(() => {
    if (!documents) return []
    const byType = new Map<string, number>()
    for (const doc of documents) byType.set(doc.type, (byType.get(doc.type) ?? 0) + 1)
    return [...byType]
      .map(([type, count]) => ({ type, count }))
      .sort((a, b) => b.count - a.count || a.type.localeCompare(b.type, 'de'))
  }, [documents])

  // A filter for a type that no longer exists would leave an empty, unexplained list.
  const activeType = counts.some((c) => c.type === typeFilter) ? typeFilter : null

  const visible = useMemo(() => {
    if (!documents) return []
    const filtered = activeType ? documents.filter((d) => d.type === activeType) : documents
    return [...filtered].sort((a, b) => compare(a, b, sort))
  }, [documents, activeType, sort])

  const reload = () => setReloads((n) => n + 1)

  function changeSort(key: SortKey) {
    setSort((current) =>
      current.key === key
        ? { key, dir: current.dir === 'asc' ? 'desc' : 'asc' }
        : { key, dir: key === 'name' || key === 'type' ? 'asc' : 'desc' },
    )
  }

  function remove(id: string) {
    deleteDocument(id)
      .then(reload)
      .catch((e: Error) => setError(e.message))
  }

  return (
    <>
      <h1 className="mt-10 text-[34px] leading-tight font-semibold">Dokumente</h1>
      <p className="mt-1 text-[15px] text-bleistift">
        Deine eigenen und alle, die mit dir geteilt wurden.
      </p>

      <label className="mt-7 block">
        <span className="sr-only">Dokumente durchsuchen</span>
        <input
          type="search"
          value={term}
          onChange={(e) => setTerm(e.target.value)}
          placeholder="Nach Namen suchen"
          className="block w-full rounded-[4px] border border-linie bg-papier px-3.5 py-2.5 text-[16px] placeholder:text-bleistift/70 focus:border-mappe focus:outline-none"
        />
      </label>

      {error ? (
        <p role="alert" className="mt-6 border-l-4 border-typ-rot bg-papier px-4 py-3 text-[15px]">
          {error} Läuft die API auf Port 8080?
        </p>
      ) : null}

      <section className="mt-8" aria-label="Dokumentliste">
        <RegisterTabs
          total={documents?.length ?? 0}
          counts={counts}
          active={activeType}
          onChange={setTypeFilter}
        />

        <div className="overflow-hidden rounded-[6px] border border-linie bg-papier">
          {!documents && !error ? (
            <p className="px-5 py-8 text-[15px] text-bleistift">Wird geladen …</p>
          ) : null}

          {documents && visible.length === 0 ? (
            <p className="px-5 py-10 text-[16px] text-bleistift">
              {debounced
                ? `Kein Dokument passt zu „${debounced}“.`
                : 'Noch keine Dokumente. Leg unten die erste Datei ab.'}
            </p>
          ) : null}

          {visible.length > 0 ? (
            <DocumentsTable documents={visible} sort={sort} onSort={changeSort} onDelete={remove} />
          ) : null}

          <UploadField onUploaded={reload} />
        </div>
      </section>
    </>
  )
}
