'use client'
import { useEffect, useRef } from 'react'
import Link from 'next/link'
import { useAuth } from '@/lib/auth'
import { PublicSurveysSection } from '@/components/public-surveys-section'
import {
  Lock, Mail, BarChart3, Eye, Copy, Users,
} from 'lucide-react'

export default function LandingPage() {
  const { user } = useAuth()
  const ctaHref = user ? '/surveys/create' : '/auth/login'

  useReveal()

  return (
    <>
      {/* ── HERO ── */}
      <section style={{
        background: 'var(--ink)',
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        position: 'relative',
        overflow: 'hidden',
        paddingTop: '64px',
      }}>
        {/* Subtle grain texture */}
        <div style={{
          position: 'absolute', inset: 0, pointerEvents: 'none', opacity: 0.03,
          backgroundImage: `url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='noise'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.9' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23noise)'/%3E%3C/svg%3E")`,
          backgroundSize: '256px 256px',
        }} />

        <div style={{
          maxWidth: '1200px',
          margin: '0 auto',
          padding: '80px 24px',
          width: '100%',
          display: 'grid',
          gridTemplateColumns: '1fr 1fr',
          gap: '64px',
          alignItems: 'center',
        }} className="hero-grid">
          {/* Left: Copy */}
          <div>
            <h1 style={{
              fontFamily: '"Playfair Display", Georgia, serif',
              fontSize: 'clamp(2.4rem, 5vw, 3.75rem)',
              fontWeight: 700,
              lineHeight: 1.1,
              color: '#FAFAF9',
              letterSpacing: '-0.02em',
              marginBottom: '24px',
            }}>
              Pesquisas que as pessoas{' '}
              <em style={{ color: 'var(--amber)', fontStyle: 'italic' }}>realmente</em>{' '}
              querem responder.
            </h1>

            <p style={{
              fontSize: '17px',
              lineHeight: 1.65,
              color: 'rgba(250, 250, 249, 0.55)',
              marginBottom: '36px',
              maxWidth: '480px',
            }}>
              Três modos de acesso — público, por código de email ou login obrigatório.
              Você decide quem pode responder. Resultados em tempo real.
            </p>

            <div style={{ display: 'flex', gap: '12px', flexWrap: 'wrap', marginBottom: '16px' }}>
              <Link href={ctaHref} style={{
                display: 'inline-flex',
                alignItems: 'center',
                padding: '14px 28px',
                background: 'var(--amber)',
                color: '#1C1917',
                fontWeight: 700,
                fontSize: '15px',
                borderRadius: '10px',
                textDecoration: 'none',
                transition: 'background 0.2s, box-shadow 0.2s',
                boxShadow: '0 4px 24px rgba(245,158,11,0.25)',
              }}
              onMouseEnter={e => {
                const el = e.currentTarget as HTMLElement
                el.style.background = 'var(--amber-hover)'
                el.style.boxShadow = '0 6px 32px rgba(245,158,11,0.35)'
              }}
              onMouseLeave={e => {
                const el = e.currentTarget as HTMLElement
                el.style.background = 'var(--amber)'
                el.style.boxShadow = '0 4px 24px rgba(245,158,11,0.25)'
              }}
              >
                Começar grátis
              </Link>
              <Link href="/explore" style={{
                display: 'inline-flex',
                alignItems: 'center',
                padding: '14px 28px',
                background: 'transparent',
                color: 'rgba(250,250,249,0.8)',
                fontWeight: 500,
                fontSize: '15px',
                borderRadius: '10px',
                border: '1px solid rgba(250,250,249,0.2)',
                textDecoration: 'none',
                transition: 'border-color 0.2s, color 0.2s',
              }}
              onMouseEnter={e => {
                const el = e.currentTarget as HTMLElement
                el.style.borderColor = 'rgba(250,250,249,0.45)'
                el.style.color = '#FAFAF9'
              }}
              onMouseLeave={e => {
                const el = e.currentTarget as HTMLElement
                el.style.borderColor = 'rgba(250,250,249,0.2)'
                el.style.color = 'rgba(250,250,249,0.8)'
              }}
              >
                Explorar pesquisas públicas
              </Link>
            </div>

          </div>

          {/* Right: App mockup */}
          <div className="hero-mockup" style={{ display: 'flex', justifyContent: 'center' }}>
            <div style={{
              transform: 'rotate(-3deg)',
              animation: 'float 6s ease-in-out infinite',
              width: '100%',
              maxWidth: '380px',
            }}>
              <div style={{
                background: 'rgba(255,255,255,0.04)',
                border: '1px solid rgba(255,255,255,0.08)',
                borderRadius: '16px',
                padding: '28px',
                boxShadow: '0 40px 80px rgba(0,0,0,0.5), 0 0 0 1px rgba(255,255,255,0.04)',
              }}>
                {/* Mock survey card */}
                <div style={{ marginBottom: '20px' }}>
                  <div style={{
                    display: 'inline-flex', alignItems: 'center', gap: '6px',
                    background: 'rgba(245,158,11,0.12)', border: '1px solid rgba(245,158,11,0.25)',
                    borderRadius: '6px', padding: '4px 10px',
                    fontSize: '11px', color: 'var(--amber)', marginBottom: '12px', fontWeight: 600,
                  }}>
                    <span style={{ width: '6px', height: '6px', borderRadius: '50%', background: 'var(--amber)', display: 'inline-block' }} />
                    Ativa
                  </div>
                  <h3 style={{
                    fontFamily: '"Playfair Display", Georgia, serif',
                    fontSize: '18px',
                    fontWeight: 600,
                    color: '#FAFAF9',
                    marginBottom: '6px',
                    lineHeight: 1.3,
                  }}>
                    Pesquisa de satisfação 2026
                  </h3>
                  <p style={{ fontSize: '13px', color: 'rgba(250,250,249,0.4)', marginBottom: '20px' }}>
                    Pergunta 1 de 3
                  </p>
                </div>

                <p style={{ fontSize: '15px', color: 'rgba(250,250,249,0.85)', marginBottom: '16px', fontWeight: 500 }}>
                  Como você avalia nossa plataforma?
                </p>

                {['Excelente — superou minhas expectativas', 'Boa — atende ao que preciso', 'Precisa melhorar'].map((opt, i) => (
                  <div key={i} style={{
                    display: 'flex', alignItems: 'center', gap: '10px',
                    padding: '11px 14px',
                    borderRadius: '8px',
                    border: `1px solid ${i === 0 ? 'rgba(245,158,11,0.5)' : 'rgba(255,255,255,0.08)'}`,
                    background: i === 0 ? 'rgba(245,158,11,0.08)' : 'transparent',
                    marginBottom: i < 2 ? '8px' : '0',
                    cursor: 'pointer',
                  }}>
                    <div style={{
                      width: '16px', height: '16px', borderRadius: '50%',
                      border: `2px solid ${i === 0 ? 'var(--amber)' : 'rgba(255,255,255,0.2)'}`,
                      background: i === 0 ? 'var(--amber)' : 'transparent',
                      flexShrink: 0,
                      display: 'flex', alignItems: 'center', justifyContent: 'center',
                    }}>
                      {i === 0 && <div style={{ width: '6px', height: '6px', borderRadius: '50%', background: '#1C1917' }} />}
                    </div>
                    <span style={{ fontSize: '13px', color: i === 0 ? '#FAFAF9' : 'rgba(250,250,249,0.5)' }}>
                      {opt}
                    </span>
                  </div>
                ))}

                <div style={{
                  marginTop: '20px', paddingTop: '20px',
                  borderTop: '1px solid rgba(255,255,255,0.06)',
                  display: 'flex', justifyContent: 'flex-end',
                }}>
                  <div style={{
                    padding: '9px 20px',
                    background: 'var(--amber)', color: '#1C1917',
                    borderRadius: '8px', fontSize: '13px', fontWeight: 700,
                  }}>
                    Próxima →
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* ── FEATURES ── */}
      <section id="features" style={{ background: 'var(--ink-soft)', padding: '96px 24px' }}>
        <div style={{ maxWidth: '1200px', margin: '0 auto' }}>
          <div className="reveal" style={{ textAlign: 'center', marginBottom: '64px' }}>
            <h2 style={{
              fontFamily: '"Playfair Display", Georgia, serif',
              fontSize: 'clamp(2rem, 4vw, 2.75rem)',
              fontWeight: 700, color: '#FAFAF9',
              letterSpacing: '-0.02em', lineHeight: 1.15,
            }}>
              Tudo que uma pesquisa precisa.
            </h2>
          </div>

          <div style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))',
            gap: '1px',
            background: 'rgba(255,255,255,0.06)',
            border: '1px solid rgba(255,255,255,0.06)',
            borderRadius: '16px',
            overflow: 'hidden',
          }}>
            {features.map((f, i) => (
              <FeatureCard key={i} {...f} />
            ))}
          </div>
        </div>
      </section>

      {/* ── HOW IT WORKS ── */}
      <section id="how-it-works" style={{ background: 'var(--paper)', padding: '96px 24px' }}>
        <div style={{ maxWidth: '960px', margin: '0 auto' }}>
          <div className="reveal" style={{ textAlign: 'center', marginBottom: '72px' }}>
            <h2 style={{
              fontFamily: '"Playfair Display", Georgia, serif',
              fontSize: 'clamp(2rem, 4vw, 2.75rem)',
              fontWeight: 700, color: 'var(--ink)',
              letterSpacing: '-0.02em', lineHeight: 1.15,
            }}>
              Três passos. Sua primeira pesquisa em minutos.
            </h2>
          </div>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '0' }}>
            {steps.map((step, i) => (
              <StepCard key={i} {...step} index={i} last={i === steps.length - 1} />
            ))}
          </div>
        </div>
      </section>

      {/* ── PUBLIC SURVEYS ── */}
      <PublicSurveysSection />

      {/* ── CTA FINAL ── */}
      <section style={{ background: 'var(--ink-soft)', padding: '112px 24px', textAlign: 'center' }}>
        <div style={{ maxWidth: '640px', margin: '0 auto' }} className="reveal">
          <h2 style={{
            fontFamily: '"Playfair Display", Georgia, serif',
            fontSize: 'clamp(2rem, 5vw, 3rem)',
            fontWeight: 700, color: '#FAFAF9',
            letterSpacing: '-0.02em', lineHeight: 1.15,
            marginBottom: '40px',
          }}>
            Pronto para sua primeira pesquisa?
          </h2>
          <Link href={ctaHref} style={{
            display: 'inline-flex', alignItems: 'center',
            padding: '16px 36px',
            background: 'var(--amber)', color: '#1C1917',
            fontWeight: 700, fontSize: '16px',
            borderRadius: '12px', textDecoration: 'none',
            transition: 'background 0.2s, box-shadow 0.2s',
            boxShadow: '0 4px 32px rgba(245,158,11,0.3)',
          }}
          onMouseEnter={e => {
            const el = e.currentTarget as HTMLElement
            el.style.background = 'var(--amber-hover)'
            el.style.boxShadow = '0 8px 40px rgba(245,158,11,0.4)'
          }}
          onMouseLeave={e => {
            const el = e.currentTarget as HTMLElement
            el.style.background = 'var(--amber)'
            el.style.boxShadow = '0 4px 32px rgba(245,158,11,0.3)'
          }}
          >
            Criar minha primeira pesquisa
          </Link>
        </div>
      </section>

      {/* ── FOOTER ── */}
      <footer style={{
        background: 'var(--ink)',
        borderTop: '1px solid rgba(255,255,255,0.06)',
        padding: '32px 24px',
      }}>
        <div style={{
          maxWidth: '1200px', margin: '0 auto',
          display: 'flex', alignItems: 'center', justifyContent: 'space-between',
          flexWrap: 'wrap', gap: '16px',
        }}>
          <p style={{ fontSize: '13px', color: 'rgba(250,250,249,0.35)' }}>
            Online Survey — um projeto de Luan Fernandes
          </p>
        </div>
      </footer>

      <style>{`
        @import url('https://fonts.googleapis.com/css2?family=Playfair+Display:ital,wght@0,600;0,700;1,600;1,700&display=swap');

        .hero-grid {
          grid-template-columns: 1fr 1fr;
        }
        @media (max-width: 768px) {
          .hero-grid {
            grid-template-columns: 1fr !important;
          }
          .hero-mockup {
            display: none !important;
          }
        }
        @media (max-width: 640px) {
          .landing-nav-links {
            display: none !important;
          }
        }
      `}</style>
    </>
  )
}

function FeatureCard({ icon, title, description }: { icon: React.ReactNode; title: string; description: string }) {
  return (
    <div
      style={{
        background: 'var(--ink-soft)',
        padding: '32px 28px',
        transition: 'background 0.2s',
      }}
      onMouseEnter={e => { (e.currentTarget as HTMLElement).style.background = 'rgba(255,255,255,0.03)' }}
      onMouseLeave={e => { (e.currentTarget as HTMLElement).style.background = 'var(--ink-soft)' }}
    >
      <div style={{
        width: '40px', height: '40px',
        background: 'rgba(245,158,11,0.1)',
        border: '1px solid rgba(245,158,11,0.2)',
        borderRadius: '10px',
        display: 'flex', alignItems: 'center', justifyContent: 'center',
        marginBottom: '16px', color: 'var(--amber)',
      }}>
        {icon}
      </div>
      <h3 style={{
        fontSize: '16px', fontWeight: 600, color: '#FAFAF9',
        marginBottom: '8px',
      }}>
        {title}
      </h3>
      <p style={{ fontSize: '14px', color: 'rgba(250,250,249,0.45)', lineHeight: 1.6 }}>
        {description}
      </p>
    </div>
  )
}

function StepCard({ number, title, description, index, last }: {
  number: string; title: string; description: string; index: number; last: boolean
}) {
  return (
    <div className="reveal" style={{
      display: 'flex', gap: '32px', alignItems: 'flex-start',
      paddingBottom: last ? '0' : '56px',
      borderBottom: last ? 'none' : '1px solid rgba(28,25,23,0.08)',
      marginBottom: last ? '0' : '56px',
    }}>
      <div style={{ flexShrink: 0 }}>
        <span style={{
          fontFamily: '"Playfair Display", Georgia, serif',
          fontSize: '48px', fontWeight: 700,
          color: 'rgba(28,25,23,0.12)', lineHeight: 1,
          display: 'block',
        }}>
          {number}
        </span>
      </div>
      <div style={{ paddingTop: '8px' }}>
        <h3 style={{
          fontFamily: '"Playfair Display", Georgia, serif',
          fontSize: '1.5rem', fontWeight: 700, color: 'var(--ink)',
          marginBottom: '10px', letterSpacing: '-0.01em',
        }}>
          {title}
        </h3>
        <p style={{ fontSize: '16px', color: 'rgba(28,25,23,0.55)', lineHeight: 1.65 }}>
          {description}
        </p>
      </div>
    </div>
  )
}

const features = [
  {
    icon: <Lock size={18} />,
    title: 'Acesso controlado',
    description: 'Três modos: público para todos, código enviado por email, ou login obrigatório. Você decide.',
  },
  {
    icon: <Mail size={18} />,
    title: 'Verificação por email',
    description: 'Códigos one-time gerados automaticamente. Expiram em 15 minutos. Sem senhas para gerenciar.',
  },
  {
    icon: <BarChart3 size={18} />,
    title: 'Resultados em tempo real',
    description: 'Acompanhe as respostas conforme chegam. Gráficos e exportação em CSV.',
  },
  {
    icon: <Eye size={18} />,
    title: 'Preview antes de publicar',
    description: 'Visualize exatamente como o respondente vai ver antes de ativar a pesquisa.',
  },
  {
    icon: <Copy size={18} />,
    title: 'Duplique pesquisas',
    description: 'Copie uma pesquisa existente com um clique. Ajuste e publique sem recriar do zero.',
  },
  {
    icon: <Users size={18} />,
    title: 'Dados do respondente',
    description: 'Colete nome, email ou idade de forma opcional e por pesquisa. Configurável individualmente.',
  },
]

const steps = [
  {
    number: '01',
    title: 'Crie sua pesquisa',
    description: 'Defina título, descrição e adicione perguntas com opções de resposta. Interface simples, sem complicação.',
  },
  {
    number: '02',
    title: 'Escolha o acesso',
    description: 'Público para qualquer pessoa, código por email para audiências específicas, ou login obrigatório para controle total.',
  },
  {
    number: '03',
    title: 'Compartilhe e acompanhe',
    description: 'Copie o link direto e distribua. Os resultados aparecem em tempo real no painel.',
  },
]

function useReveal() {
  const observed = useRef(false)
  useEffect(() => {
    if (observed.current) return
    observed.current = true

    const elements = document.querySelectorAll<HTMLElement>('.reveal')
    if (!elements.length) return

    const vh = window.innerHeight
    const pending: HTMLElement[] = []

    // Reveal immediately anything already in or above the viewport
    elements.forEach(el => {
      const rect = el.getBoundingClientRect()
      if (rect.top < vh * 0.9) {
        el.classList.add('visible')
      } else {
        pending.push(el)
      }
    })

    if (!pending.length) return

    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry, i) => {
          if (entry.isIntersecting) {
            const el = entry.target as HTMLElement
            const delay = (el.dataset.delay ? parseInt(el.dataset.delay) : i * 80)
            setTimeout(() => el.classList.add('visible'), delay)
            observer.unobserve(el)
          }
        })
      },
      { threshold: 0.15 }
    )

    pending.forEach(el => observer.observe(el))
    return () => observer.disconnect()
  }, [])
}
