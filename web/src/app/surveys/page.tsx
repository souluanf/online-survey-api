'use client'
import { useEffect, useMemo, useState } from 'react'
import { useAuth } from '@/lib/auth'
import { api } from '@/lib/api'
import { SurveyResponse, PaginatedResponse, SurveyDetailResponse } from '@/lib/types'
import { AuthGuard } from '@/components/auth/auth-guard'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { LinkButton } from '@/components/ui/link-button'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import {
  Plus, BarChart2, PlayCircle, XCircle, Trash2, Pencil, Copy, Eye, Link as LinkIcon,
  ChevronLeft, ChevronRight, FileText, CheckCircle2, Search
} from 'lucide-react'

function statusLabel(s: string) {
  if (s === 'Draft') return 'Rascunho'
  if (s === 'Active') return 'Ativa'
  if (s === 'Closed') return 'Encerrada'
  return s
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
  const [surveys, setSurveys] = useState<SurveyResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)
  const [actionInfo, setActionInfo] = useState<string | null>(null)
  const [search, setSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState<string>('all')
  const [page, setPage] = useState(1)
  const [pageSize] = useState(10)
  const [totalPages, setTotalPages] = useState(1)
  const [totalCount, setTotalCount] = useState(0)

  const load = async () => {
    setLoading(true)
    try {
      const token = await getToken()
      if (!token) return
      const data: PaginatedResponse<SurveyResponse> = await api.surveys.list(token, page, pageSize)
      setSurveys(data.items)
      setTotalPages(data.totalPages)
      setTotalCount(data.totalCount)
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erro ao carregar pesquisas')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [page])

  const counters = useMemo(() => {
    const active = surveys.filter(s => s.status === 'Active').length
    const draft = surveys.filter(s => s.status === 'Draft').length
    const closed = surveys.filter(s => s.status === 'Closed').length
    const responses = surveys.reduce((acc, s) => acc + s.responseCount, 0)
    return { active, draft, closed, responses }
  }, [surveys])

  const filtered = useMemo(() => {
    return surveys.filter(s => {
      if (statusFilter !== 'all' && s.status !== statusFilter) return false
      if (search.trim() && !s.title.toLowerCase().includes(search.toLowerCase())) return false
      return true
    })
  }, [surveys, search, statusFilter])

  const flash = (msg: string) => {
    setActionInfo(msg)
    setTimeout(() => setActionInfo(null), 3000)
  }

  const wrap = async (fn: () => Promise<void>, errMsg: string) => {
    setActionError(null)
    try { await fn() } catch (e) {
      setActionError(e instanceof Error ? e.message : errMsg)
    }
  }

  const handleActivate = (id: string) => wrap(async () => {
    const token = await getToken(); if (!token) return
    await api.surveys.activate(token, id, {})
    await load()
  }, 'Erro ao ativar')

  const handleClose = (id: string) => wrap(async () => {
    const token = await getToken(); if (!token) return
    await api.surveys.close(token, id)
    await load()
  }, 'Erro ao encerrar')

  const handleDelete = (id: string) => {
    if (!confirm('Tem certeza que deseja excluir esta pesquisa?')) return
    wrap(async () => {
      const token = await getToken(); if (!token) return
      await api.surveys.delete(token, id)
      setSurveys(prev => prev.filter(s => s.id !== id))
    }, 'Erro ao excluir')
  }

  const handleDuplicate = (id: string) => wrap(async () => {
    const token = await getToken(); if (!token) return
    const detail: SurveyDetailResponse = await api.surveys.get(id, token)
    const collectedFields = typeof detail.collectedFields === 'number' ? detail.collectedFields : 0
    const payload = {
      title: `${detail.title} (cópia)`,
      description: detail.description ?? undefined,
      accessMode: detail.accessMode === 'CodeByEmail' ? 1 : detail.accessMode === 'RequiresLogin' ? 2 : 0,
      isPublic: detail.isPublic,
      collectedFields,
      questions: detail.questions.map((q, qi) => ({
        text: q.text,
        order: qi + 1,
        isRequired: q.isRequired,
        options: q.options.map((o, oi) => ({ text: o.text, order: oi + 1 })),
      })),
    }
    await api.surveys.create(token, payload)
    flash('Pesquisa duplicada com sucesso')
    await load()
  }, 'Erro ao duplicar')

  const handleCopyLink = async (id: string) => {
    const url = `${window.location.origin}/surveys/answer?id=${id}`
    await navigator.clipboard.writeText(url)
    flash('Link copiado')
  }

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold">Minhas Pesquisas</h1>
        <LinkButton href="/surveys/create" className="bg-indigo-600 hover:bg-indigo-700">
          <Plus className="w-4 h-4 mr-1" />Nova Pesquisa
        </LinkButton>
      </div>

      <div className="grid grid-cols-2 md:grid-cols-4 gap-3 mb-6">
        <StatCard icon={<FileText className="w-4 h-4" />} label="Total" value={totalCount} />
        <StatCard icon={<CheckCircle2 className="w-4 h-4 text-green-600" />} label="Ativas" value={counters.active} />
        <StatCard icon={<Pencil className="w-4 h-4 text-zinc-500" />} label="Rascunho" value={counters.draft} />
        <StatCard icon={<BarChart2 className="w-4 h-4 text-indigo-600" />} label="Respostas (página)" value={counters.responses} />
      </div>

      <div className="flex flex-col md:flex-row gap-3 mb-4">
        <div className="relative flex-1">
          <Search className="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-zinc-400" />
          <Input
            value={search}
            onChange={e => setSearch(e.target.value)}
            placeholder="Buscar por título..."
            className="pl-9"
          />
        </div>
        <Select value={statusFilter} onValueChange={v => setStatusFilter(v ?? 'all')}>
          <SelectTrigger className="md:w-48">
            <SelectValue>
              {(v) => {
                if (v === 'Draft') return 'Rascunho'
                if (v === 'Active') return 'Ativa'
                if (v === 'Closed') return 'Encerrada'
                return 'Todos os status'
              }}
            </SelectValue>
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Todos os status</SelectItem>
            <SelectItem value="Draft">Rascunho</SelectItem>
            <SelectItem value="Active">Ativa</SelectItem>
            <SelectItem value="Closed">Encerrada</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {error && <Alert variant="destructive" className="mb-4"><AlertDescription>{error}</AlertDescription></Alert>}
      {actionError && <Alert variant="destructive" className="mb-4"><AlertDescription>{actionError}</AlertDescription></Alert>}
      {actionInfo && <Alert className="mb-4 border-green-300 bg-green-50 text-green-800"><AlertDescription>{actionInfo}</AlertDescription></Alert>}

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
            {surveys.length === 0 ? (
              <>
                <p className="mb-4">Nenhuma pesquisa criada ainda.</p>
                <LinkButton href="/surveys/create" className="bg-indigo-600 hover:bg-indigo-700">
                  Criar primeira pesquisa
                </LinkButton>
              </>
            ) : (
              <p>Nenhum resultado para o filtro aplicado.</p>
            )}
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-3">
          {filtered.map(survey => (
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
                  <LinkButton href={`/surveys/results?id=${survey.id}`} variant="outline" size="sm">
                    <BarChart2 className="w-3 h-3 mr-1" />Resultados
                  </LinkButton>
                  <LinkButton href={`/surveys/answer?id=${survey.id}`} variant="outline" size="sm">
                    <Eye className="w-3 h-3 mr-1" />Pré-visualizar
                  </LinkButton>
                  {survey.status === 'Active' && (
                    <Button size="sm" variant="outline" onClick={() => handleCopyLink(survey.id)}>
                      <LinkIcon className="w-3 h-3 mr-1" />Copiar link
                    </Button>
                  )}
                  <LinkButton href={`/surveys/edit?id=${survey.id}`} variant="outline" size="sm">
                    <Pencil className="w-3 h-3 mr-1" />Editar
                  </LinkButton>
                  <Button size="sm" variant="outline" onClick={() => handleDuplicate(survey.id)}>
                    <Copy className="w-3 h-3 mr-1" />Duplicar
                  </Button>
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
                  {survey.status !== 'Active' && (
                    <Button size="sm" variant="outline" className="text-red-700 border-red-300 hover:bg-red-50" onClick={() => handleDelete(survey.id)}>
                      <Trash2 className="w-3 h-3 mr-1" />Excluir
                    </Button>
                  )}
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {totalPages > 1 && (
        <div className="flex items-center justify-between mt-6">
          <p className="text-sm text-zinc-500">Página {page} de {totalPages}</p>
          <div className="flex gap-2">
            <Button size="sm" variant="outline" onClick={() => setPage(p => Math.max(1, p - 1))} disabled={page <= 1}>
              <ChevronLeft className="w-4 h-4 mr-1" />Anterior
            </Button>
            <Button size="sm" variant="outline" onClick={() => setPage(p => Math.min(totalPages, p + 1))} disabled={page >= totalPages}>
              Próxima<ChevronRight className="w-4 h-4 ml-1" />
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}

function StatCard({ icon, label, value }: { icon: React.ReactNode, label: string, value: number }) {
  return (
    <Card>
      <CardContent className="py-4">
        <div className="flex items-center gap-2 text-sm text-zinc-500 mb-1">
          {icon}<span>{label}</span>
        </div>
        <p className="text-2xl font-bold">{value}</p>
      </CardContent>
    </Card>
  )
}
