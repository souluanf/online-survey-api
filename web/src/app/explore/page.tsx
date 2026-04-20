'use client'
import { useEffect, useState } from 'react'
import { api } from '@/lib/api'
import { SurveyResponse } from '@/lib/types'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { LinkButton } from '@/components/ui/link-button'
import { Skeleton } from '@/components/ui/skeleton'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Input } from '@/components/ui/input'
import { Search, ExternalLink } from 'lucide-react'
export default function ExplorePage() {
  const [surveys, setSurveys] = useState<SurveyResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [search, setSearch] = useState('')

  useEffect(() => {
    api.surveys.active()
      .then((list: SurveyResponse[]) => setSurveys(list.filter(s => s.isPublic)))
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false))
  }, [])

  const filtered = surveys.filter(s =>
    !search.trim() || s.title.toLowerCase().includes(search.toLowerCase())
  )

  return (
    <>
      {/* Dark hero band */}
      <div
        className="px-4 pt-28 pb-16"
        style={{ background: 'var(--ink)' }}
      >
        <div className="max-w-4xl mx-auto">
          <p className="text-xs font-semibold uppercase tracking-widest mb-3" style={{ color: 'var(--amber)' }}>
            Pesquisas públicas
          </p>
          <h1
            className="font-heading text-3xl md:text-4xl font-bold mb-2"
            style={{ color: '#FAFAF9', letterSpacing: '-0.02em', lineHeight: 1.1 }}
          >
            Explorar pesquisas
          </h1>
          <p className="text-sm mt-2" style={{ color: 'rgba(250,250,249,0.5)' }}>
            Pesquisas ativas abertas ao público — escolha uma para responder.
          </p>

          <div className="relative mt-6 max-w-md">
            <Search className="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2" style={{ color: 'rgba(250,250,249,0.4)' }} />
            <Input
              value={search}
              onChange={e => setSearch(e.target.value)}
              placeholder="Buscar por título..."
              className="pl-9 bg-white/8 border-white/12 text-[#FAFAF9] placeholder:text-white/30 focus-visible:ring-[var(--amber)]/50 focus-visible:border-[var(--amber)]/60"
            />
          </div>
        </div>
      </div>

      {/* Card grid on paper bg */}
      <div className="px-4 py-8" style={{ background: 'var(--paper)' }}>
        <div className="max-w-4xl mx-auto">
          {error && <Alert variant="destructive" className="mb-4"><AlertDescription>{error}</AlertDescription></Alert>}

          {loading ? (
            <div className="grid gap-4 md:grid-cols-2">
              {[1, 2, 3, 4].map(i => (
                <Card key={i} className="border-zinc-200">
                  <CardHeader><Skeleton className="h-5 w-48" /></CardHeader>
                  <CardContent><Skeleton className="h-4 w-32" /></CardContent>
                </Card>
              ))}
            </div>
          ) : filtered.length === 0 ? (
            <Card className="border-zinc-200">
              <CardContent className="py-12 text-center text-zinc-400">
                {surveys.length === 0
                  ? 'Nenhuma pesquisa pública ativa no momento.'
                  : 'Nenhum resultado para a busca.'}
              </CardContent>
            </Card>
          ) : (
            <div className="grid gap-4 md:grid-cols-2">
              {filtered.map(s => (
                <Card
                  key={s.id}
                  className="border-zinc-200 bg-white hover:border-amber-300 transition-colors group"
                >
                  <CardHeader className="pb-2">
                    <CardTitle
                      className="font-heading text-base font-semibold leading-snug"
                      style={{ color: 'var(--ink)' }}
                    >
                      {s.title}
                    </CardTitle>
                  </CardHeader>
                  <CardContent className="space-y-3">
                    {s.description && (
                      <p className="text-sm text-zinc-500 line-clamp-2">{s.description}</p>
                    )}
                    <div className="flex gap-3 text-xs text-zinc-400">
                      <span>{s.questionCount} perguntas</span>
                      <span>{s.responseCount} respostas</span>
                    </div>
                    <LinkButton href={`/surveys/answer?id=${s.id}`} size="sm">
                      <ExternalLink className="w-3 h-3 mr-1" />Responder
                    </LinkButton>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </div>
      </div>
    </>
  )
}
