import { typeColor } from '../lib/fileTypes'

export interface TypeCount {
  type: string
  count: number
}

type Orientation = 'horizontal' | 'vertical'

interface Props {
  total: number
  counts: TypeCount[]
  active: string | null
  onChange: (type: string | null) => void
  orientation?: Orientation
  className?: string
}

interface TabProps {
  label: string
  count: number
  color: string
  selected: boolean
  onSelect: () => void
  orientation: Orientation
}

const SHAPE: Record<Orientation, { base: string; on: string; off: string; strip: string }> = {
  horizontal: {
    base: 'shrink-0 rounded-t-[6px] px-4',
    on: 'border-b-papier pt-3 pb-2',
    off: 'border-b-linie pt-2 pb-1.5',
    strip: 'inset-x-[-1px] top-[-1px] h-[3px] rounded-t-[6px]',
  },
  vertical: {
    base: 'flex items-baseline justify-between rounded-l-[6px] py-2 pr-3 pl-4 text-left',
    on: 'w-full border-r-papier',
    off: 'w-[calc(100%-0.75rem)] border-r-linie',
    strip: 'inset-y-[-1px] left-[-1px] w-[3px] rounded-l-[6px]',
  },
}

function Tab({ label, count, color, selected, onSelect, orientation }: TabProps) {
  const shape = SHAPE[orientation]
  return (
    <button
      type="button"
      aria-pressed={selected}
      onClick={onSelect}
      className={`relative border border-linie font-condensed text-[16px] font-medium transition-colors motion-reduce:transition-none ${shape.base} ${
        selected
          ? `bg-papier text-graphit ${shape.on}`
          : `bg-reiter text-bleistift hover:text-graphit ${shape.off}`
      }`}
    >
      <span aria-hidden className={`absolute ${shape.strip}`} style={{ backgroundColor: color }} />
      <span className="min-w-0 truncate">{label}</span>
      <span className="ml-2 font-sans text-[13px] tabular-nums text-bleistift">{count}</span>
    </button>
  )
}

export default function RegisterTabs({
  total,
  counts,
  active,
  onChange,
  orientation = 'horizontal',
  className = '',
}: Props) {
  const layout =
    orientation === 'horizontal'
      ? '-mb-px flex items-end gap-1 overflow-x-auto px-2 pt-1 sm:px-3'
      : '-mr-px flex flex-col items-end gap-1 pt-3'

  return (
    <div className={`relative z-10 ${layout} ${className}`}>
      <Tab
        label="Alle"
        count={total}
        color="var(--color-graphit)"
        selected={active === null}
        onSelect={() => onChange(null)}
        orientation={orientation}
      />
      {counts.map(({ type, count }) => (
        <Tab
          key={type}
          label={`.${type}`}
          count={count}
          color={typeColor(type)}
          selected={active === type}
          onSelect={() => onChange(type)}
          orientation={orientation}
        />
      ))}
    </div>
  )
}
