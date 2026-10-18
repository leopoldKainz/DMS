const GROUPS: { color: string; ink: string; types: string[] }[] = [
  { color: 'var(--color-typ-rot)', ink: '#fff', types: ['pdf'] },
  { color: 'var(--color-typ-blau)', ink: '#fff', types: ['doc', 'docx', 'odt', 'rtf', 'txt', 'md'] },
  { color: 'var(--color-typ-gruen)', ink: '#fff', types: ['xls', 'xlsx', 'ods', 'csv'] },
  {
    color: 'var(--color-typ-ocker)',
    ink: 'var(--color-graphit)',
    types: ['png', 'jpg', 'jpeg', 'gif', 'svg', 'webp', 'heic'],
  },
]

const FALLBACK = { color: 'var(--color-typ-grau)', ink: 'var(--color-graphit)' }

function group(type: string) {
  const normalized = type.toLowerCase()
  return GROUPS.find((g) => g.types.includes(normalized)) ?? FALLBACK
}

export const typeColor = (type: string) => group(type).color
export const typeInk = (type: string) => group(type).ink
