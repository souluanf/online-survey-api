'use client'
import { useEffect, useState } from 'react'
import Link from 'next/link'
import { api } from '@/lib/api'
import { SurveyResponse } from '@/lib/types'
import { ArrowRight } from 'lucide-react'

export function PublicSurveysSection() {
  const [surveys, setSurveys] = useState<SurveyResponse[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    api.surveys.active()
      .then((list: SurveyResponse[]) => setSurveys(list.filter(s => s.isPublic).slice(0, 6)))
      .catch(() => { /* silent on landing */ })
      .finally(() => setLoading(false))
  }, [])

  if (!loading && surveys.length === 0) return null

  return (
    <section style={{ background: 'var(--paper)', padding: '96px 24px', borderTop: '1px solid #E7E5E4' }}>
      <div style={{ maxWidth: '1100px', margin: '0 auto' }}>
        <div className="reveal" style={{ textAlign: 'center', marginBottom: '56px' }}>
          <p style={{
            fontSize: '11px', fontWeight: 600, letterSpacing: '0.12em',
            color: 'var(--amber)', textTransform: 'uppercase', marginBottom: '12px',
          }}>
            Abertas agora
          </p>
          <h2 style={{
            fontFamily: '"Playfair Display", Georgia, serif',
            fontSize: 'clamp(2rem, 4vw, 2.75rem)',
            fontWeight: 700, color: 'var(--ink)',
            letterSpacing: '-0.02em', lineHeight: 1.15,
            marginBottom: '12px',
          }}>
            Participe de pesquisas públicas
          </h2>
          <p style={{ color: '#78716C', fontSize: '1rem', maxWidth: '520px', margin: '0 auto' }}>
            Responda pesquisas ativas da comunidade. Anônimo, rápido e sem cadastro.
          </p>
        </div>

        <div
          className="reveal"
          style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))',
            gap: '20px',
            marginBottom: '48px',
          }}
        >
          {loading
            ? [1, 2, 3].map(i => (
                <div key={i} className="animate-pulse" style={{
                  background: '#F5F5F4', border: '1px solid #E7E5E4',
                  borderRadius: '12px', padding: '24px', height: '160px',
                }} />
              ))
            : surveys.map(s => (
                <Link
                  key={s.id}
                  href={`/surveys/answer?id=${s.id}`}
                  className="group"
                  style={{
                    background: '#fff',
                    border: '1px solid #E7E5E4',
                    borderRadius: '12px',
                    padding: '24px',
                    display: 'flex',
                    flexDirection: 'column',
                    gap: '12px',
                    textDecoration: 'none',
                    transition: 'all 0.2s ease',
                  }}
                  onMouseEnter={e => {
                    e.currentTarget.style.borderColor = 'var(--amber)'
                    e.currentTarget.style.transform = 'translateY(-2px)'
                    e.currentTarget.style.boxShadow = '0 8px 24px -8px rgba(245, 158, 11, 0.25)'
                  }}
                  onMouseLeave={e => {
                    e.currentTarget.style.borderColor = '#E7E5E4'
                    e.currentTarget.style.transform = 'translateY(0)'
                    e.currentTarget.style.boxShadow = 'none'
                  }}
                >
                  <h3 style={{
                    fontFamily: '"Playfair Display", Georgia, serif',
                    fontSize: '1.125rem', fontWeight: 600, color: 'var(--ink)',
                    lineHeight: 1.3, letterSpacing: '-0.01em',
                  }}>
                    {s.title}
                  </h3>
                  {s.description && (
                    <p style={{
                      color: '#78716C', fontSize: '0.875rem', lineHeight: 1.5,
                      display: '-webkit-box', WebkitLineClamp: 2,
                      WebkitBoxOrient: 'vertical', overflow: 'hidden',
                    }}>
                      {s.description}
                    </p>
                  )}
                  <div style={{
                    marginTop: 'auto', display: 'flex', gap: '16px',
                    fontSize: '0.75rem', color: '#A8A29E',
                  }}>
                    <span>{s.questionCount} perguntas</span>
                    <span>{s.responseCount} respostas</span>
                  </div>
                </Link>
              ))}
        </div>

        <div className="reveal" style={{ textAlign: 'center' }}>
          <Link
            href="/explore"
            style={{
              display: 'inline-flex', alignItems: 'center', gap: '8px',
              color: 'var(--ink)', fontSize: '0.9375rem', fontWeight: 600,
              textDecoration: 'none', padding: '12px 24px',
              border: '1px solid var(--ink)', borderRadius: '999px',
              transition: 'all 0.2s ease',
            }}
            onMouseEnter={e => {
              e.currentTarget.style.background = 'var(--ink)'
              e.currentTarget.style.color = '#FAFAF9'
            }}
            onMouseLeave={e => {
              e.currentTarget.style.background = 'transparent'
              e.currentTarget.style.color = 'var(--ink)'
            }}
          >
            Ver todas as pesquisas <ArrowRight className="w-4 h-4" />
          </Link>
        </div>
      </div>
    </section>
  )
}
