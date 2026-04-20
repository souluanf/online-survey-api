'use client'
import { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '@/lib/auth'
import { api } from '@/lib/api'
import { SurveyResponse, PaginatedResponse } from '@/lib/types'
import { AuthGuard } from '@/components/auth/auth-guard'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { LinkButton } from '@/components/ui/link-button'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Plus, BarChart2, PlayCircle, XCircle, Trash2 } from 'lucide-react'

function statusLabel(s: string) {
  if (s === 'Draft') return 'Rascunho'
  if (s === 'Active') return 'Ativa'
  return 'Encerrada'
}

function StatusBadge({ status }: { status: string }) {
  if (status === 'Active') return <Badge className="bg-green-100 text-green-700 hover:bg-green-100">Ativa</Badge>
  if (status === 'Closed') return <Badge className="bg-red-100 text-red-700 hover:bg-red-100">Encerrada</Badge>
  return <Badge variant="secondary">{statusLabel(status)}</Badge>
}

function formatDate(d?: string) {
  if (!d) return '—'
  return new Date(d).toLocaleDateString('pt-BR')
}

export default function SurveysPage() {
  return (
    <AuthGuard>
      <SurveysList />
    </AuthGuard>
  )
}

function SurveysList() {
  const { getToken } = useAuth()
  const router = useRouter()
  const [surveys, setSurveys] = useState<SurveyResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)

  const load = async () => {
    setLoading(true)
    try {
      const token = await getToken()
      if (!token) return
      const data: PaginatedResponse<SurveyResponse> = await api.surveys.list(token)
      setSurveys(data.items)
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erro ao carregar pesquisas')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  const handleActivate = async (id: string) => {
    setActionError(null)
    try {
      const token = await getToken()
      if (!token) return
      await api.surveys.activate(token, id, {})
      await load()
    } catch (e) {
      setActionError(e instanceof Error ? e.message : 'Erro ao ativar')
    }
  }

  const handleClose = async (id: string) => {
    setActionError(null)
    try {
      const token = await getToken()
      if (!token) return
      await api.surveys.close(token, id)
      await load()
    } catch (e) {
      setActionError(e instanceof Error ? e.message : 'Erro ao encerrar')
    }
  }

  const handleDelete = async (id: string) => {
    if (!confirm('Tem certeza que deseja excluir esta pesquisa?')) return
    setActionError(null)
    try {
      const token = await getToken()
      if (!token) return
      await api.surveys.delete(token, id)
      setSurveys(prev => prev.filter(s => s.id !== id))
    } catch (e) {
      setActionError(e instanceof Error ? e.message : 'Erro ao excluir')
    }
  }

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold">Minhas Pesquisas</h1>
        <LinkButton href="/surveys/create" className="bg-indigo-600 hover:bg-indigo-700">
          <Plus className="w-4 h-4 mr-1" />Nova Pesquisa
        </LinkButton>
      </div>

      {error && <Alert variant="destructive" className="mb-4"><AlertDescription>{error}</AlertDescription></Alert>}
      {actionError && <Alert variant="destructive" className="mb-4"><AlertDescription>{actionError}</AlertDescription></Alert>}

      {loading ? (
        <div className="space-y-3">
          {[1, 2, 3].map(i => (
            <Card key={i}>
              <CardHeader><Skeleton className="h-5 w-48" /></CardHeader>
              <CardContent><Skeleton className="h-4 w-32" /></CardContent>
            </Card>
          ))}
        </div>
      ) : surveys.length === 0 ? (
        <Card>
          <CardContent className="py-12 text-center text-zinc-500">
            <p className="mb-4">Nenhuma pesquisa criada ainda.</p>
            <LinkButton href="/surveys/create" className="bg-indigo-600 hover:bg-indigo-700">
              Criar primeira pesquisa
            </LinkButton>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-3">
          {surveys.map(survey => (
            <Card key={survey.id}>
              <CardHeader className="pb-2">
                <div className="flex items-start justify-between gap-2">
                  <CardTitle className="text-base font-semibold">{survey.title}</CardTitle>
                  <StatusBadge status={survey.status} />
                </div>
              </CardHeader>
              <CardContent>
                <div className="flex flex-wrap gap-4 text-sm text-zinc-500 mb-4">
                  <span>{survey.questionCount} perguntas</span>
                  <span>{survey.responseCount} respostas</span>
                  <span>Criada em {formatDate(survey.createdAt)}</span>
                  {survey.startDate && <span>Início: {formatDate(survey.startDate)}</span>}
                  {survey.endDate && <span>Fim: {formatDate(survey.endDate)}</span>}
                </div>
                <div className="flex flex-wrap gap-2">
                  <LinkButton href={`/surveys/${survey.id}/results`} variant="outline" size="sm">
                    <BarChart2 className="w-3 h-3 mr-1" />Resultados
                  </LinkButton>
                  {survey.status === 'Draft' && (
                    <Button size="sm" variant="outline" className="text-green-700 border-green-300 hover:bg-green-50" onClick={() => handleActivate(survey.id)}>
                      <PlayCircle className="w-3 h-3 mr-1" />Ativar
                    </Button>
                  )}
                  {survey.status === 'Active' && (
                    <Button size="sm" variant="outline" className="text-orange-700 border-orange-300 hover:bg-orange-50" onClick={() => handleClose(survey.id)}>
                      <XCircle className="w-3 h-3 mr-1" />Encerrar
                    </Button>
                  )}
                  <Button size="sm" variant="outline" className="text-red-700 border-red-300 hover:bg-red-50" onClick={() => handleDelete(survey.id)}>
                    <Trash2 className="w-3 h-3 mr-1" />Excluir
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}
