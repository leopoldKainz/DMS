import { useState } from 'react'
import { Link } from 'react-router'
import type { DocumentDto } from '../api/documents'
import { formatDate, formatSize } from '../lib/format'
import { typeColor } from '../lib/fileTypes'

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
  { key: 'name', label: 'Name', className: 'pl-5 text-left' },
  { key: 'type', label: 'Typ', className: 'hidden w-20 pl-4 text-left sm:table-cell' },
  { key: 'size', label: 'Größe', className: 'hidden w-24 pl-4 text-right sm:table-cell' },
  { key: 'createdAt', label: 'Hinzugefügt', className: 'hidden w-40 pl-4 text-right md:table-cell' },
]

interface Props {
  documents: DocumentDto[]
  sort: Sort
  onSort: (key: SortKey) => void
  onDelete: (id: string) => void
}

export default function DocumentsTable({ documents, sort, onSort, onDelete }: Props) {
  const [confirming, setConfirming] = useState<string | null>(null)

  return (
    <table className="w-full border-collapse">
      <thead>
        <tr className="border-b border-linie text-[13px] text-bleistift">
          {COLUMNS.map((col) => (
            <th
              key={col.key}
              scope="col"
              aria-sort={
                sort.key === col.key ? (sort.dir === 'asc' ? 'ascending' : 'descending') : 'none'
              }
              className={`py-2.5 font-medium ${col.className}`}
            >
              <button type="button" onClick={() => onSort(col.key)} className="hover:text-graphit">
                {col.label}
                {sort.key === col.key ? (
                  <span aria-hidden> {sort.dir === 'asc' ? '↑' : '↓'}</span>
                ) : null}
              </button>
            </th>
          ))}
          <th scope="col" className="w-36 py-2.5 pr-5">
            <span className="sr-only">Aktionen</span>
          </th>
        </tr>
      </thead>

      <tbody>
        {documents.map((doc) => (
          <tr key={doc.id} className="border-b border-linie last:border-b-0 hover:bg-ablage/60">
            <td
              className="py-3.5 pr-4 pl-5 align-top"
              style={{ boxShadow: `inset 4px 0 0 ${typeColor(doc.type)}` }}
            >
              <Link to={`/documents/${doc.id}`} className="text-[16px] font-medium hover:underline">
                {doc.name}
              </Link>
              <span className="mt-0.5 block text-[13px] tabular-nums text-bleistift sm:hidden">
                .{doc.type}, {formatSize(doc.size)}
              </span>
            </td>
            <td className="hidden py-3.5 pl-4 align-top font-condensed text-[16px] text-bleistift sm:table-cell">
              .{doc.type}
            </td>
            <td className="hidden py-3.5 pl-4 text-right align-top text-[14px] tabular-nums text-bleistift sm:table-cell">
              {formatSize(doc.size)}
            </td>
            <td className="hidden py-3.5 pl-4 text-right align-top text-[14px] tabular-nums text-bleistift md:table-cell">
              {formatDate(doc.createdAt)}
            </td>
            <td className="py-3.5 pr-5 pl-4 text-right align-top text-[14px] whitespace-nowrap">
              {confirming === doc.id ? (
                <>
                  <button
                    type="button"
                    onClick={() => {
                      setConfirming(null)
                      onDelete(doc.id)
                    }}
                    className="font-medium text-typ-rot hover:underline"
                  >
                    Löschen
                  </button>
                  <button
                    type="button"
                    onClick={() => setConfirming(null)}
                    className="ml-3 text-bleistift hover:text-graphit"
                  >
                    Behalten
                  </button>
                </>
              ) : (
                <button
                  type="button"
                  onClick={() => setConfirming(doc.id)}
                  className="text-bleistift hover:text-typ-rot"
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
