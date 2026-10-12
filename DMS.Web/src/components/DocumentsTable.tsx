import { useState } from 'react'
import { Link } from 'react-router'
import type { DocumentDto } from '../api/documents'
import { formatDate, formatSize } from '../lib/format'

export type SortKey = 'name' | 'type' | 'size' | 'createdAt'

export interface Sort {
  key: SortKey
  dir: 'asc' | 'desc'
}

interface Column {
  key: SortKey
  label: string
  className: string
}

const COLUMNS: Column[] = [
  { key: 'type', label: 'Endung', className: 'w-16 pl-3 text-left sm:w-24 sm:pl-4' },
  { key: 'name', label: 'Name', className: 'text-left' },
  { key: 'size', label: 'Größe', className: 'hidden pl-6 text-right sm:table-cell' },
  { key: 'createdAt', label: 'Erstellt', className: 'hidden w-44 pl-6 text-right sm:table-cell' },
]

interface Props {
  documents: DocumentDto[]
  sort: Sort
  onSort: (key: SortKey) => void
  onDelete: (id: number) => void
}

export default function DocumentsTable({ documents, sort, onSort, onDelete }: Props) {
  const [confirming, setConfirming] = useState<number | null>(null)

  return (
    <table className="w-full border-collapse text-left">
      <thead>
        <tr className="border-b border-rule text-[13px] text-quiet">
          {COLUMNS.map((col) => (
            <th
              key={col.key}
              scope="col"
              aria-sort={
                sort.key === col.key ? (sort.dir === 'asc' ? 'ascending' : 'descending') : 'none'
              }
              className={`py-2 font-normal ${col.className}`}
            >
              <button
                type="button"
                onClick={() => onSort(col.key)}
                className="hover:text-stamp focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-stamp"
              >
                {col.label}
                {sort.key === col.key ? (
                  <span aria-hidden> {sort.dir === 'asc' ? '↑' : '↓'}</span>
                ) : null}
              </button>
            </th>
          ))}
          <th scope="col" className="w-28 py-2 pl-4 text-right font-normal">
            <span className="sr-only">Aktionen</span>
          </th>
        </tr>
      </thead>

      <tbody>
        {documents.map((doc) => (
          <tr key={doc.id} className="group border-b border-rule">
            <td className="border-l-2 border-transparent py-5 pl-3 align-top font-serif text-[18px] leading-snug text-stamp group-hover:border-stamp sm:pl-4">
              .{doc.type}
            </td>
            <td className="py-5 pr-4 align-top">
              <Link
                to={`/documents/${doc.id}`}
                className="font-serif text-[18px] leading-snug decoration-stamp decoration-1 underline-offset-[5px] group-hover:underline focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-stamp"
              >
                {doc.name}
              </Link>
              <span className="mt-1 block text-[13px] tabular-nums text-quiet sm:hidden">
                {formatSize(doc.size)}
                <span className="inline-block w-5" />
                {formatDate(doc.createdAt)}
              </span>
            </td>
            <td className="hidden py-5 pl-6 text-right align-top text-[13px] tabular-nums text-quiet sm:table-cell">
              {formatSize(doc.size)}
            </td>
            <td className="hidden py-5 pl-6 text-right align-top text-[13px] tabular-nums text-quiet sm:table-cell">
              {formatDate(doc.createdAt)}
            </td>
            <td className="py-5 pl-4 text-right align-top text-[13px]">
              {confirming === doc.id ? (
                <span className="whitespace-nowrap">
                  <button
                    type="button"
                    onClick={() => {
                      setConfirming(null)
                      onDelete(doc.id)
                    }}
                    className="text-stamp underline underline-offset-4 focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-stamp"
                  >
                    Löschen
                  </button>
                  <button
                    type="button"
                    onClick={() => setConfirming(null)}
                    className="ml-3 text-quiet hover:text-ink focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-stamp"
                  >
                    Abbrechen
                  </button>
                </span>
              ) : (
                <button
                  type="button"
                  onClick={() => setConfirming(doc.id)}
                  className="text-quiet hover:text-stamp focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-stamp"
                >
                  Löschen
                </button>
              )}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
