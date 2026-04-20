'use client'
import { usePathname } from 'next/navigation'
import { useAuth } from '@/lib/auth'
import { Navbar } from '@/components/navbar'
import { LandingNav } from '@/components/landing-nav'

export function ConditionalNavbar() {
  const pathname = usePathname()
  const { user, loading } = useAuth()
  if (loading) return null
  if (!user) return <LandingNav />
  if (pathname === '/' || pathname === '/explore') return <LandingNav />
  return <Navbar />
}
