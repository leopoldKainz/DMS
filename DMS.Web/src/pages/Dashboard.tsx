import { useEffect, useMemo, useState } from 'react'
import { useOutletContext } from 'react-router'
import {
  deleteDocument,
  listDocuments,
  searchDocuments,
  type DocumentDto,
} from '../api/documents'
import type { ShellContext } from '../components/AppShell'
import DocumentsTable, { type Sort, type SortKey } from '../components/DocumentsTable'
import UploadField from '../components/UploadField'
import { formatSize } from '../lib/format'

function compare(a: DocumentDto, b: DocumentDto, sort: Sort): number {
  const dir = sort.dir === 'asc' ? 1 : -1
  if (sort.key === 'size') return (a.size - b.size) * dir
  if (sort.key === 'createdAt') {
    return (Date.parse(a.createdAt) - Date.parse(b.createdAt)) * dir
  }
  return a[sort.key].localeCompare(b[sort.key], 'de') * dir
}

export default function Dashboard() {
  const { userId } = useOutletContext<ShellContext>()

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
    const pending = debounced ? searchDocuments(debounced, userId) : listDocuments(userId)

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
  }, [debounced, userId, reloads])

  const types = useMemo(() => {
    if (!documents) return []
    return [...new Set(documents.map((d) => d.type))].sort((a, b) => a.localeCompare(b, 'de'))
  }, [documents])

  const visible = useMemo(() => {
    if (!documents) return []
    const filtered = typeFilter ? documents.filter((d) => d.type === typeFilter) : documents
    return [...filtered].sort((a, b) => compare(a, b, sort))
  }, [documents, typeFilter, sort])

  const totalSize = useMemo(
    () => (documents ? documents.reduce((sum, d) => sum + d.size, 0) : 0),
    [documents],
  )

  const reload = () => setReloads((n) => n + 1)

  function changeSort(key: SortKey) {
    setSort((current) =>
      current.key === key
        ? { key, dir: current.dir === 'asc' ? 'desc' : 'asc' }
        : { key, dir: key === 'name' || key === 'type' ? 'asc' : 'desc' },
    )
  }

  function remove(id: number) {
    deleteDocument(id)
      .then(reload)
      .catch((e: Error) => setError(e.message))
  }

  return (
    <>
      <h1 className="mt-12 font-serif text-[30px] leading-none">Dokumente</h1>

      <dl className="mt-8 flex flex-wrap gap-x-12 gap-y-6 border-b border-rule pb-8">
        <div>
          <dd className="font-serif text-[26px] leading-none tabular-nums">
            {documents ? documents.length : '—'}
          </dd>
          <dt className="mt-1.5 text-[13px] text-quiet">
            {documents?.length === 1 ? 'Eintrag' : 'Einträge'}
          </dt>
        </div>
        <div>
          <dd className="font-serif text-[26px] leading-none tabular-nums">
            {documents ? formatSize(totalSize) : '—'}
          </dd>
          <dt className="mt-1.5 text-[13px] text-quiet">Belegt</dt>
        </div>
        <div>
          <dd className="font-serif text-[26px] leading-none tabular-nums">
            {documents ? types.length : '—'}
          </dd>
          <dt className="mt-1.5 text-[13px] text-quiet">
            {types.length === 1 ? 'Dateityp' : 'Dateitypen'}
          </dt>
        </div>
      </dl>

      <div className="mt-8 flex flex-wrap items-end justify-between gap-x-8 gap-y-5">
        <label className="min-w-56 flex-1 text-[13px] text-quiet">
          Suche
          <input
            type="search"
            value={term}
            onChange={(e) => setTerm(e.target.value)}
            placeholder="Name eines Dokuments"
            className="mt-1 block w-full border-b border-rule bg-transparent pb-1.5 text-[15px] text-ink placeholder:text-quiet/60 focus:border-stamp focus:outline-none"
          />
        </label>
        <UploadField userId={userId} onUploaded={reload} />
      </div>

      {types.length > 1 ? (
        <div className="mt-6 flex flex-wrap items-center gap-x-5 gap-y-2 text-[13px]">
          <button
            type="button"
            onClick={() => setTypeFilter(null)}
            className={
              typeFilter === null
                ? 'border-b border-stamp pb-0.5 text-stamp'
                : 'border-b border-transparent pb-0.5 text-quiet hover:text-ink'
            }
          >
            Alle
          </button>
          {types.map((type) => (
            <button
              key={type}
              type="button"
              onClick={() => setTypeFilter(type)}
              className={
                typeFilter === type
                  ? 'border-b border-stamp pb-0.5 text-stamp'
                  : 'border-b border-transparent pb-0.5 text-quiet hover:text-ink'
              }
            >
              .{type}
            </button>
          ))}
        </div>
      ) : null}

      {error ? (
        <p className="mt-10 border-l-2 border-stamp py-1 pl-4 text-sm leading-relaxed">
          {error} Prüfe, ob der API-Container läuft.
        </p>
      ) : null}

      {!documents && !error ? <p className="mt-10 text-[13px] text-quiet">Wird geladen</p> : null}

      {documents && visible.length === 0 ? (
        <p className="mt-12 max-w-[38ch] font-serif text-[19px] leading-relaxed text-quiet">
          {debounced
            ? `Kein Eintrag passt zu „${debounced}“.`
            : 'Noch nichts im Register. Lade eine Datei hoch, um den ersten Eintrag anzulegen.'}
        </p>
      ) : null}

      {visible.length > 0 ? (
        <div className="mt-8">
          <DocumentsTable
            documents={visible}
            sort={sort}
            onSort={changeSort}
            onDelete={remove}
          />
        </div>
      ) : null}
    </>
  )
}
