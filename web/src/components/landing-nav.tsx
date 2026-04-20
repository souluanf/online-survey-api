'use client'
import { useEffect, useState } from 'react'
import Link from 'next/link'
import { usePathname, useRouter } from 'next/navigation'
import { useAuth } from '@/lib/auth'

export function LandingNav() {
  const { user, loading } = useAuth()
  const pathname = usePathname()
  const router = useRouter()
  // On routes without a dark hero, keep the nav opaque from the start
  const forceOpaque = pathname !== '/' && pathname !== '/explore'
  const [scrolled, setScrolled] = useState(forceOpaque)

  useEffect(() => {
    if (forceOpaque) { setScrolled(true); return }
    const update = () => setScrolled(window.scrollY > 20)
    update() // initial sync (handles anchor-jump loads)
    window.addEventListener('scroll', update, { passive: true })
    return () => window.removeEventListener('scroll', update)
  }, [forceOpaque])

  useEffect(() => {
    // Scroll to hash after navigating to /. Poll briefly until the target exists.
    if (pathname !== '/' || !window.location.hash) return
    const id = window.location.hash.slice(1)
    let tries = 0
    const attempt = () => {
      const el = document.getElementById(id)
      if (el) {
        el.scrollIntoView({ behavior: 'smooth', block: 'start' })
      } else if (tries++ < 20) {
        requestAnimationFrame(attempt)
      }
    }
    requestAnimationFrame(attempt)
  }, [pathname])

  const handleAnchor = (e: React.MouseEvent, id: string) => {
    if (pathname === '/') {
      e.preventDefault()
      document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' })
      history.replaceState(null, '', `#${id}`)
    } else {
      e.preventDefault()
      router.push(`/#${id}`)
    }
  }

  const loginHref = user ? '/surveys' : '/auth/login'
  const ctaHref = user ? '/surveys/create' : '/auth/login'

  return (
    <header
      style={{
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        zIndex: 50,
        transition: 'background 0.3s ease, backdrop-filter 0.3s ease, box-shadow 0.3s ease',
        background: scrolled ? 'rgba(28, 25, 23, 0.95)' : 'transparent',
        backdropFilter: scrolled ? 'blur(12px)' : 'none',
        WebkitBackdropFilter: scrolled ? 'blur(12px)' : 'none',
        boxShadow: scrolled ? '0 1px 0 rgba(255,255,255,0.06)' : 'none',
      }}
    >
      <div style={{
        maxWidth: '1200px',
        margin: '0 auto',
        padding: '0 24px',
        height: '64px',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '40px' }}>
          <Link href="/" style={{
            fontFamily: 'Georgia, serif',
            fontWeight: 700,
            fontSize: '18px',
            color: '#FAFAF9',
            textDecoration: 'none',
            letterSpacing: '-0.02em',
          }}>
            Online Survey
          </Link>
          <nav style={{ display: 'flex', gap: '28px' }} className="landing-nav-links">
            <Link href="/#features" onClick={e => handleAnchor(e, 'features')} style={navLinkStyle}>Recursos</Link>
            <Link href="/#how-it-works" onClick={e => handleAnchor(e, 'how-it-works')} style={navLinkStyle}>Como funciona</Link>
            <Link href="/explore" style={navLinkStyle}>Explorar pesquisas</Link>
          </nav>
        </div>

        {!loading && (
          <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
            <Link href={loginHref} style={{
              fontSize: '14px',
              fontWeight: 500,
              color: 'rgba(250, 250, 249, 0.75)',
              textDecoration: 'none',
              padding: '8px 16px',
              borderRadius: '8px',
              border: '1px solid rgba(250, 250, 249, 0.15)',
              transition: 'color 0.2s, border-color 0.2s',
            }}
            onMouseEnter={e => {
              (e.target as HTMLElement).style.color = '#FAFAF9';
              (e.target as HTMLElement).style.borderColor = 'rgba(250,250,249,0.35)';
            }}
            onMouseLeave={e => {
              (e.target as HTMLElement).style.color = 'rgba(250,250,249,0.75)';
              (e.target as HTMLElement).style.borderColor = 'rgba(250,250,249,0.15)';
            }}
            >
              {user ? 'Ir para o app' : 'Entrar'}
            </Link>
            {!user && <Link href={ctaHref} style={{
              fontSize: '14px',
              fontWeight: 600,
              color: '#1C1917',
              textDecoration: 'none',
              padding: '8px 18px',
              borderRadius: '8px',
              background: '#F59E0B',
              transition: 'background 0.2s',
            }}
            onMouseEnter={e => { (e.target as HTMLElement).style.background = '#D97706' }}
            onMouseLeave={e => { (e.target as HTMLElement).style.background = '#F59E0B' }}
            >
              Começar grátis
            </Link>}
          </div>
        )}
      </div>
    </header>
  )
}

const navLinkStyle: React.CSSProperties = {
  fontSize: '14px',
  fontWeight: 400,
  color: 'rgba(250, 250, 249, 0.65)',
  textDecoration: 'none',
  transition: 'color 0.2s',
}
