import type { ReactNode } from 'react'

interface Props {
  title: string
  intro: string
  children: ReactNode
  footer: ReactNode
}

export default function AuthLayout({ title, intro, children, footer }: Props) {
  return (
    <div className="min-h-screen">
      <header className="border-b border-linie bg-papier">
        <div className="mx-auto max-w-5xl px-4 py-3 sm:px-8">
          <span className="font-condensed text-[19px] font-semibold tracking-[0.01em]">DMS</span>
        </div>
      </header>

      <main className="mx-auto max-w-sm px-4 pt-16 pb-24 sm:pt-24">
        <h1 className="text-[34px] leading-tight font-semibold">{title}</h1>
        <p className="mt-2 text-[15px] leading-relaxed text-bleistift">{intro}</p>

        <div className="mt-8 rounded-[6px] border border-linie bg-papier p-6">{children}</div>

        <p className="mt-6 text-[14px] text-bleistift">{footer}</p>
      </main>
    </div>
  )
}
