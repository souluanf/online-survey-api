'use client'
import { Suspense, useEffect, useState } from 'react'
import { useSearchParams, useRouter } from 'next/navigation'
import { useAuth } from '@/lib/auth'
import { api } from '@/lib/api'
import { SurveyDetailResponse } from '@/lib/types'
import { AuthGuard } from '@/components/auth/auth-guard'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { LinkButton } from '@/components/ui/link-button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Skeleton } from '@/components/ui/skeleton'
import { ArrowLeft } from 'lucide-react'

export default function EditSurveyPage() {
  return (
    <AuthGuard>
      <Suspense fallback={<Skeleton className="h-32 w-full" />}>
        <EditForm />
      </Suspense>
    </AuthGuard>
  )
}

function EditForm() {
  const searchParams = useSearchParams()
  const id = searchParams.get('id') ?? ''
  const { getToken } = useAuth()
  const router = useRouter()

  const [survey, setSurvey] = useState<SurveyDetailResponse | null>(null)
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!id) { setLoading(false); setError('ID da pesquisa não informado'); return }
    api.surveys.get(id)
      .then((s: SurveyDetailResponse) => {
        setSurvey(s)
        setTitle(s.title)
        setDescription(s.description ?? '')
      })
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false))
  }, [id])

  const handleSave = async () => {
    if (!title.trim()) return setError('Título é obrigatório')
    setSaving(true)
    setError(null)
    try {
      const token = await getToken()
      if (!token) return
      await api.surveys.update(token, id, { title, description: description || null })
      router.push('/surveys')
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erro ao salvar')
    } finally {
      setSaving(false)
    }
  }

  if (loading) return <Skeleton className="h-32 w-full" />
  if (error && !survey) return <Alert variant="destructive"><AlertDescription>{error}</AlertDescription></Alert>

  if (survey && survey.status !== 'Draft') return (
    <div className="space-y-4 max-w-2xl">
      <LinkButton href="/surveys" variant="outline" size="sm">
        <ArrowLeft className="w-4 h-4 mr-1" />Voltar
      </LinkButton>
      <Alert className="border-amber-300 bg-amber-50 text-amber-900">
        <AlertDescription>
          Apenas pesquisas em rascunho podem ser editadas. Esta pesquisa está {survey.status === 'Active' ? 'ativa' : 'encerrada'}.
        </AlertDescription>
      </Alert>
    </div>
  )

  return (
    <div className="space-y-6 max-w-2xl">
      <LinkButton href="/surveys" variant="outline" size="sm">
        <ArrowLeft className="w-4 h-4 mr-1" />Voltar
      </LinkButton>

      <h1 className="text-2xl font-bold">Editar Pesquisa</h1>

      {error && <Alert variant="destructive"><AlertDescription>{error}</AlertDescription></Alert>}

      <Card>
        <CardHeader><CardTitle className="text-base">Informações</CardTitle></CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-1">
            <Label>Título *</Label>
            <Input value={title} onChange={e => setTitle(e.target.value)} />
          </div>
          <div className="space-y-1">
            <Label>Descrição</Label>
            <Textarea value={description} onChange={e => setDescription(e.target.value)} rows={3} />
          </div>
          <p className="text-xs text-zinc-500">
            Perguntas e opções não podem ser editadas após a criação para preservar a integridade das respostas.
          </p>
        </CardContent>
      </Card>

      <div className="flex gap-3">
        <Button onClick={handleSave} disabled={saving} className="bg-indigo-600 hover:bg-indigo-700">
          {saving ? 'Salvando...' : 'Salvar'}
        </Button>
        <Button variant="outline" onClick={() => router.push('/surveys')}>Cancelar</Button>
      </div>
    </div>
  )
}
