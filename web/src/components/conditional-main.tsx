'use client'
import { usePathname } from 'next/navigation'
import { useAuth } from '@/lib/auth'

export function ConditionalMain({ children }: { children: React.ReactNode }) {
  const pathname = usePathname()
  const { user } = useAuth()
  if (pathname === '/' || pathname === '/explore') return <>{children}</>
  // When logged out, LandingNav (fixed, 64px) is shown — offset main
  const topPad = !user ? 'pt-24' : 'py-8'
  return (
    <main className={`container mx-auto px-4 ${topPad} pb-8 max-w-4xl`}>
      {children}
    </main>
  )
}
