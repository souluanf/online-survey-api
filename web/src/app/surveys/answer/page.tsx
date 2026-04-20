'use client'
import { Suspense, useEffect, useState } from 'react'
import { useSearchParams, useRouter } from 'next/navigation'
import { useAuth } from '@/lib/auth'
import { api } from '@/lib/api'
import { SurveyDetailResponse } from '@/lib/types'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { LinkButton } from '@/components/ui/link-button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Skeleton } from '@/components/ui/skeleton'
import { CheckCircle2 } from 'lucide-react'

type AccessState = 'loading' | 'email' | 'code' | 'granted' | 'denied' | 'inactive' | 'done'

export default function SurveyAnswerPage() {
  return (
    <Suspense fallback={<Skeleton className="h-32 w-full" />}>
      <SurveyAnswerContent />
    </Suspense>
  )
}

function SurveyAnswerContent() {
  const searchParams = useSearchParams()
  const id = searchParams.get('id') ?? ''
  const preview = searchParams.get('preview') === '1'
  const { user, loading: authLoading } = useAuth()
  const router = useRouter()

  const [survey, setSurvey] = useState<SurveyDetailResponse | null>(null)
  const [accessState, setAccessState] = useState<AccessState>('loading')
  const [email, setEmail] = useState('')
  const [code, setCode] = useState('')
  const [accessToken, setAccessToken] = useState<string | null>(null)
  const [answers, setAnswers] = useState<Record<string, string>>({})
  const [actionError, setActionError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    if (!id) { setAccessState('denied'); return }
    if (authLoading) return
    api.surveys.get(id)
      .then((s: SurveyDetailResponse) => {
        setSurvey(s)
        if (preview) {
          setAccessState('granted')
          return
        }
        if (s.status !== 'Active') {
          setAccessState('inactive')
          return
        }
        if (s.accessMode === 'RequiresLogin') {
          if (!user) { router.push('/auth/login'); return }
          setAccessState('granted')
        } else if (s.accessMode === 'CodeByEmail') {
          setAccessState('email')
        } else {
          setAccessState('granted')
        }
      })
      .catch(() => setAccessState('denied'))
  }, [id, user, authLoading, router, preview])

  const handleRequestCode = async () => {
    setActionError(null)
    try {
      await api.surveys.requestCode(id, email)
      setAccessState('code')
    } catch (e) {
      setActionError(e instanceof Error ? e.message : 'Erro ao solicitar código')
    }
  }

  const handleVerifyCode = async () => {
    setActionError(null)
    try {
      const res = await api.surveys.verifyCode(id, email, code)
      setAccessToken(res?.token ?? null)
      setAccessState('granted')
    } catch (e) {
      setActionError(e instanceof Error ? e.message : 'Código inválido')
    }
  }

  const handleSubmit = async () => {
    if (!survey) return
    setActionError(null)
    setSubmitting(true)
    try {
      const payload = {
        surveyId: id,
        accessToken: accessToken ?? undefined,
        answers: survey.questions.map(q => ({
          questionId: q.id,
          selectedOptionId: answers[q.id],
        })).filter(a => a.selectedOptionId),
      }
      await api.responses.submit(payload)
      setAccessState('done')
    } catch (e) {
      setActionError(e instanceof Error ? e.message : 'Erro ao enviar respostas')
    } finally {
      setSubmitting(false)
    }
  }

  if (accessState === 'loading') return (
    <div className="space-y-4">
      <Skeleton className="h-8 w-64" />
      <Skeleton className="h-32 w-full" />
    </div>
  )

  if (accessState === 'inactive') return (
    <Card className="max-w-md mx-auto mt-12 border-zinc-200">
      <CardContent className="py-10 text-center text-zinc-400">
        Esta pesquisa não está disponível no momento.
      </CardContent>
    </Card>
  )

  if (accessState === 'denied') return (
    <Card className="max-w-md mx-auto mt-12 border-zinc-200">
      <CardContent className="py-10 text-center text-zinc-400">
        Pesquisa não encontrada.
      </CardContent>
    </Card>
  )

  if (accessState === 'done') return (
    <Card className="max-w-md mx-auto mt-12 border-zinc-200">
      <CardContent className="py-10 text-center space-y-4">
        <CheckCircle2 className="w-12 h-12 mx-auto" style={{ color: 'var(--amber)' }} />
        <p className="font-heading text-xl font-semibold" style={{ color: 'var(--ink)' }}>
          Obrigado pela sua resposta!
        </p>
        <p className="text-sm text-zinc-400">Sua participação foi registrada com sucesso.</p>
      </CardContent>
    </Card>
  )

  if (accessState === 'email') return (
    <Card className="max-w-md mx-auto mt-12 border-zinc-200">
      <CardHeader>
        <CardTitle className="font-heading text-lg" style={{ color: 'var(--ink)' }}>
          Acesso por email
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <p className="text-sm text-zinc-500">Informe seu email para receber o código de acesso.</p>
        <div className="space-y-1">
          <Label>Email</Label>
          <Input type="email" value={email} onChange={e => setEmail(e.target.value)} placeholder="seu@email.com" />
        </div>
        {actionError && <Alert variant="destructive"><AlertDescription>{actionError}</AlertDescription></Alert>}
        <Button className="w-full" onClick={handleRequestCode}>
          Enviar código
        </Button>
      </CardContent>
    </Card>
  )

  if (accessState === 'code') return (
    <Card className="max-w-md mx-auto mt-12 border-zinc-200">
      <CardHeader>
        <CardTitle className="font-heading text-lg" style={{ color: 'var(--ink)' }}>
          Verificar código
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <p className="text-sm text-zinc-500">Informe o código enviado para <strong>{email}</strong>.</p>
        <div className="space-y-1">
          <Label>Código</Label>
          <Input value={code} onChange={e => setCode(e.target.value)} placeholder="000000" />
        </div>
        {actionError && <Alert variant="destructive"><AlertDescription>{actionError}</AlertDescription></Alert>}
        <div className="flex gap-2">
          <Button className="flex-1" onClick={handleVerifyCode}>
            Verificar
          </Button>
          <Button variant="outline" onClick={() => setAccessState('email')}>Voltar</Button>
        </div>
      </CardContent>
    </Card>
  )

  if (!survey) return null

  return (
    <div className="space-y-6">
      <div>
        <h1
          className="font-heading text-2xl md:text-3xl font-bold"
          style={{ color: 'var(--ink)', letterSpacing: '-0.02em' }}
        >
          {survey.title}
        </h1>
        {survey.description && <p className="text-zinc-500 mt-1">{survey.description}</p>}
      </div>

      {preview && (
        <Alert className="border-amber-200 bg-amber-50 text-amber-900">
          <AlertDescription className="flex items-center justify-between gap-3">
            <span>Modo pré-visualização — o envio de respostas está desabilitado.</span>
            <LinkButton href="/surveys" variant="outline" size="sm">Sair do preview</LinkButton>
          </AlertDescription>
        </Alert>
      )}

      {actionError && <Alert variant="destructive"><AlertDescription>{actionError}</AlertDescription></Alert>}

      {survey.questions.map(q => (
        <Card key={q.id} className="border-zinc-200 bg-white">
          <CardHeader>
            <CardTitle className="text-base font-medium" style={{ color: 'var(--ink)' }}>
              {q.text}
              {q.isRequired && <span className="text-red-400 ml-1">*</span>}
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            {q.options.map(o => (
              <label
                key={o.id}
                className="flex items-center gap-3 cursor-pointer p-2.5 rounded-lg hover:bg-amber-50 transition-colors group"
              >
                <input
                  type="radio"
                  name={`q-${q.id}`}
                  value={o.id}
                  checked={answers[q.id] === o.id}
                  onChange={() => setAnswers(prev => ({ ...prev, [q.id]: o.id }))}
                  className="accent-amber-500"
                />
                <span className="text-sm text-zinc-700">{o.text}</span>
              </label>
            ))}
          </CardContent>
        </Card>
      ))}

      <Button
        onClick={handleSubmit}
        disabled={submitting || preview}
      >
        {preview ? 'Envio desabilitado (preview)' : submitting ? 'Enviando...' : 'Enviar Respostas'}
      </Button>
    </div>
  )
}
