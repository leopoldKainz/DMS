import type { ReactNode } from 'react'

interface Props {
  title: string
  intro: string
  children: ReactNode
  footer: ReactNode
}

export default function AuthLayout({ title, intro, children, footer }: Props) {
  return (
    <div className="min-h-screen bg-paper text-ink">
      <div className="mx-auto max-w-5xl px-5 pt-8 sm:px-8">
        <div className="flex items-center gap-x-5">
          <span className="shrink-0 font-serif text-[15px]">Dokumentenverwaltung</span>
          <span className="hidden h-px flex-1 bg-rule sm:block" />
        </div>
      </div>

      <main className="mx-auto max-w-sm px-5 pb-24 pt-20 sm:pt-28">
        <h1 className="font-serif text-[30px] leading-none">{title}</h1>
        <p className="mt-4 text-[15px] leading-relaxed text-quiet">{intro}</p>

        <div className="mt-10">{children}</div>

        <p className="mt-10 border-t border-rule pt-6 text-[13px] text-quiet">{footer}</p>
      </main>
    </div>
  )
}
