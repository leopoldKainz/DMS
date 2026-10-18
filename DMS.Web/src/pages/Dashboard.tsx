import { useEffect, useMemo, useState } from 'react'
import {
  deleteDocument,
  listDocuments,
  searchDocuments,
  type DocumentDto,
} from '../api/documents'
import { ApiError } from '../api/http'
import DocumentInfo from '../components/DocumentInfo'
import DocumentSheet from '../components/DocumentSheet'
import DocumentsTable, { type Sort, type SortKey } from '../components/DocumentsTable'
import RegisterTabs, { type TypeCount } from '../components/RegisterTabs'
import UploadField from '../components/UploadField'
import { useMediaQuery } from '../lib/useMediaQuery'

function describeError(e: Error): string {
  const unreachable = !(e instanceof ApiError) || e.status >= 502
  return unreachable ? 'Die API ist nicht erreichbar. Läuft sie auf Port 8080?' : e.message
}

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
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const wide = useMediaQuery('(min-width: 80rem)')

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
        setError(describeError(e))
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

  const activeType = counts.some((c) => c.type === typeFilter) ? typeFilter : null

  const visible = useMemo(() => {
    if (!documents) return []
    const filtered = activeType ? documents.filter((d) => d.type === activeType) : documents
    return [...filtered].sort((a, b) => compare(a, b, sort))
  }, [documents, activeType, sort])

  const selected = visible.find((d) => d.id === selectedId) ?? null

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
      .catch((e: Error) => setError(describeError(e)))
  }

  const tabs = {
    total: documents?.length ?? 0,
    counts,
    active: activeType,
    onChange: setTypeFilter,
  }

  return (
    <div className="grid grid-cols-1 pt-8 [grid-template-areas:'title''search''tabs''list''upload'] lg:grid-cols-[14rem_minmax(0,1fr)] lg:grid-rows-[auto_auto_1fr] lg:[grid-template-areas:'title_search''tabs_list''upload_list'] xl:grid-cols-[14rem_minmax(0,1fr)_22rem] xl:[grid-template-areas:'title_search_search''tabs_list_detail''upload_list_detail'] 2xl:grid-cols-[15rem_minmax(0,1fr)_26rem]">
      <div className="mb-5 [grid-area:title] lg:mr-6 lg:self-end">
        <h1 className="text-[34px] leading-tight font-semibold">Dokumente</h1>
        <p className="mt-1 text-[15px] text-bleistift lg:text-[14px]">
          Deine eigenen und alle, die mit dir geteilt wurden.
        </p>
      </div>

      <div className="mb-5 [grid-area:search] lg:self-end">
        <label className="block">
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
          <p role="alert" className="mt-4 border-l-4 border-typ-rot bg-papier px-4 py-3 text-[15px]">
            {error}
          </p>
        ) : null}
      </div>

      <div className="[grid-area:tabs]">
        <RegisterTabs {...tabs} className="lg:hidden" />
        <RegisterTabs {...tabs} orientation="vertical" className="hidden lg:flex" />
      </div>

      <section
        aria-label="Dokumentliste"
        className="overflow-hidden rounded-[6px] border border-linie bg-papier [grid-area:list] lg:min-h-[28rem]"
      >
        {!documents && !error ? (
          <p className="px-5 py-8 text-[15px] text-bleistift">Wird geladen …</p>
        ) : null}

        {documents && visible.length === 0 ? (
          <p className="px-5 py-10 text-[16px] text-bleistift">
            {debounced
              ? `Kein Dokument passt zu „${debounced}“.`
              : 'Noch keine Dokumente. Zieh die erste Datei ins Ablagefeld.'}
          </p>
        ) : null}

        {visible.length > 0 ? (
          <DocumentsTable
            documents={visible}
            sort={sort}
            onSort={changeSort}
            onDelete={remove}
            selectedId={wide ? selected?.id : null}
            onSelect={wide ? setSelectedId : undefined}
          />
        ) : null}
      </section>

      <UploadField onUploaded={reload} className="mt-4 [grid-area:upload] lg:mt-6 lg:mr-6 lg:self-start" />

      <aside aria-label="Vorschau" className="hidden [grid-area:detail] xl:ml-8 xl:block">
        <div className="sticky top-6">
          {selected ? (
            <>
              <DocumentSheet doc={selected} className="max-w-[13rem]" />
              <div className="mt-6">
                <DocumentInfo
                  key={selected.id}
                  doc={selected}
                  heading="h2"
                  onDelete={() => remove(selected.id)}
                />
              </div>
            </>
          ) : (
            <p className="border-l-4 border-linie py-1 pl-4 text-[15px] text-bleistift">
              Klick auf einen Dokumentnamen, um es hier anzusehen.
            </p>
          )}
        </div>
      </aside>
    </div>
  )
}
