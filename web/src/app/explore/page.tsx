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
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Explorar Pesquisas</h1>
        <p className="text-zinc-500 mt-1">Pesquisas públicas ativas — escolha uma para responder.</p>
      </div>

      <div className="relative">
        <Search className="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-zinc-400" />
        <Input
          value={search}
          onChange={e => setSearch(e.target.value)}
          placeholder="Buscar por título..."
          className="pl-9"
        />
      </div>

      {error && <Alert variant="destructive"><AlertDescription>{error}</AlertDescription></Alert>}

      {loading ? (
        <div className="space-y-3">
          {[1, 2, 3].map(i => (
            <Card key={i}>
              <CardHeader><Skeleton className="h-5 w-48" /></CardHeader>
              <CardContent><Skeleton className="h-4 w-32" /></CardContent>
            </Card>
          ))}
        </div>
      ) : filtered.length === 0 ? (
        <Card>
          <CardContent className="py-12 text-center text-zinc-500">
            {surveys.length === 0
              ? 'Nenhuma pesquisa pública ativa no momento.'
              : 'Nenhum resultado para a busca.'}
          </CardContent>
        </Card>
      ) : (
        <div className="grid gap-3 md:grid-cols-2">
          {filtered.map(s => (
            <Card key={s.id}>
              <CardHeader className="pb-2">
                <CardTitle className="text-base">{s.title}</CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                {s.description && <p className="text-sm text-zinc-600 line-clamp-2">{s.description}</p>}
                <div className="flex gap-3 text-xs text-zinc-500">
                  <span>{s.questionCount} perguntas</span>
                  <span>{s.responseCount} respostas</span>
                </div>
                <LinkButton href={`/surveys/answer?id=${s.id}`} size="sm" className="bg-indigo-600 hover:bg-indigo-700">
                  <ExternalLink className="w-3 h-3 mr-1" />Responder
                </LinkButton>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}
