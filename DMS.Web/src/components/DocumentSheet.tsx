import type { DocumentDto } from '../api/documents'
import { typeColor, typeInk } from '../lib/fileTypes'

interface Props {
  doc: DocumentDto
  className?: string
}

export default function DocumentSheet({ doc, className = '' }: Props) {
  const color = typeColor(doc.type)
  return (
    <div className={`relative w-full pt-7 ${className}`} aria-hidden>
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
