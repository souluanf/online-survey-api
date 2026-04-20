'use client'
import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '@/lib/auth'
import { api } from '@/lib/api'
import { AuthGuard } from '@/components/auth/auth-guard'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Checkbox } from '@/components/ui/checkbox'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Plus, Trash2 } from 'lucide-react'

interface Option {
  text: string
}

interface Question {
  text: string
  isRequired: boolean
  options: Option[]
}

export default function CreateSurveyPage() {
  return (
    <AuthGuard>
      <CreateSurveyForm />
    </AuthGuard>
  )
}

function CreateSurveyForm() {
  const { getToken } = useAuth()
  const router = useRouter()

  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [accessMode, setAccessMode] = useState<string | null>('0')
  const [isPublic, setIsPublic] = useState(false)
  const [collectedFields, setCollectedFields] = useState({ name: false, email: false, age: false })
  const [questions, setQuestions] = useState<Question[]>([
    { text: '', isRequired: false, options: [{ text: '' }, { text: '' }] }
  ])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [createdId, setCreatedId] = useState<string | null>(null)

  const addQuestion = () => {
    setQuestions(prev => [...prev, { text: '', isRequired: false, options: [{ text: '' }, { text: '' }] }])
  }

  const removeQuestion = (qi: number) => {
    setQuestions(prev => prev.filter((_, i) => i !== qi))
  }

  const updateQuestion = (qi: number, field: keyof Question, value: unknown) => {
    setQuestions(prev => prev.map((q, i) => i === qi ? { ...q, [field]: value } : q))
  }

  const addOption = (qi: number) => {
    setQuestions(prev => prev.map((q, i) => i === qi ? { ...q, options: [...q.options, { text: '' }] } : q))
  }

  const removeOption = (qi: number, oi: number) => {
    setQuestions(prev => prev.map((q, i) => i === qi ? { ...q, options: q.options.filter((_, j) => j !== oi) } : q))
  }

  const updateOption = (qi: number, oi: number, value: string) => {
    setQuestions(prev => prev.map((q, i) => i === qi ? {
      ...q,
      options: q.options.map((o, j) => j === oi ? { text: value } : o)
    } : q))
  }

  const getCollectedFieldsMask = () => {
    let mask = 0
    if (collectedFields.name) mask |= 1
    if (collectedFields.email) mask |= 2
    if (collectedFields.age) mask |= 4
    return mask
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    if (!title.trim()) return setError('Título é obrigatório')
    for (const q of questions) {
      if (!q.text.trim()) return setError('Todas as perguntas precisam de texto')
      if (q.options.length < 2) return setError('Cada pergunta precisa de pelo menos 2 opções')
      if (q.options.some(o => !o.text.trim())) return setError('Todas as opções precisam de texto')
    }
    setLoading(true)
    try {
      const token = await getToken()
      if (!token) return
      const payload = {
        title,
        description: description || undefined,
        accessMode: parseInt(accessMode ?? '0'),
        isPublic,
        collectedFields: getCollectedFieldsMask(),
        questions: questions.map((q, qi) => ({
          text: q.text,
          order: qi + 1,
          isRequired: q.isRequired,
          options: q.options.map((o, oi) => ({ text: o.text, order: oi + 1 })),
        })),
      }
      const result = await api.surveys.create(token, payload)
      setCreatedId(result.id)
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erro ao criar pesquisa')
    } finally {
      setLoading(false)
    }
  }

  const handleActivate = async () => {
    if (!createdId) return
    const token = await getToken()
    if (!token) return
    try {
      await api.surveys.activate(token, createdId, {})
      router.push('/surveys')
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erro ao ativar')
    }
  }

  if (createdId) {
    return (
      <Card className="max-w-md mx-auto mt-12 border-zinc-200">
        <CardHeader>
          <CardTitle className="font-heading text-xl" style={{ color: 'var(--ink)' }}>
            Pesquisa criada!
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          <p className="text-zinc-500">O que deseja fazer agora?</p>
          <div className="flex gap-3">
            <Button onClick={handleActivate}>
              Ativar pesquisa
            </Button>
            <Button variant="outline" onClick={() => router.push('/surveys')}>
              Voltar à lista
            </Button>
          </div>
        </CardContent>
      </Card>
    )
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div className="flex items-center justify-between">
        <h1
          className="font-heading text-2xl md:text-3xl font-bold"
          style={{ color: 'var(--ink)', letterSpacing: '-0.02em' }}
        >
          Nova Pesquisa
        </h1>
      </div>

      {error && <Alert variant="destructive"><AlertDescription>{error}</AlertDescription></Alert>}

      <Card className="border-zinc-200">
        <CardHeader>
          <CardTitle className="text-base font-semibold text-zinc-700">Informações</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-1">
            <Label>Título *</Label>
            <Input value={title} onChange={e => setTitle(e.target.value)} placeholder="Título da pesquisa" />
          </div>
          <div className="space-y-1">
            <Label>Descrição</Label>
            <Textarea value={description} onChange={e => setDescription(e.target.value)} placeholder="Descrição opcional" rows={3} />
          </div>
        </CardContent>
      </Card>

      <Card className="border-zinc-200">
        <CardHeader>
          <CardTitle className="text-base font-semibold text-zinc-700">Configurações de Acesso</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-1">
            <Label>Modo de acesso</Label>
            <Select value={accessMode} onValueChange={setAccessMode}>
              <SelectTrigger>
                <SelectValue>
                  {(v) => {
                    if (v === '1') return 'Código por email'
                    if (v === '2') return 'Requer login'
                    return 'Público anônimo'
                  }}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="0">Público anônimo</SelectItem>
                <SelectItem value="1">Código por email</SelectItem>
                <SelectItem value="2">Requer login</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div className="flex items-center gap-2">
            <Checkbox id="isPublic" checked={isPublic} onCheckedChange={v => setIsPublic(!!v)} />
            <Label htmlFor="isPublic">Pesquisa pública</Label>
          </div>
          <div className="space-y-2">
            <Label>Dados coletados do respondente</Label>
            <div className="flex gap-4">
              <div className="flex items-center gap-2">
                <Checkbox id="cf-name" checked={collectedFields.name} onCheckedChange={v => setCollectedFields(p => ({ ...p, name: !!v }))} />
                <Label htmlFor="cf-name">Nome</Label>
              </div>
              <div className="flex items-center gap-2">
                <Checkbox id="cf-email" checked={collectedFields.email} onCheckedChange={v => setCollectedFields(p => ({ ...p, email: !!v }))} />
                <Label htmlFor="cf-email">Email</Label>
              </div>
              <div className="flex items-center gap-2">
                <Checkbox id="cf-age" checked={collectedFields.age} onCheckedChange={v => setCollectedFields(p => ({ ...p, age: !!v }))} />
                <Label htmlFor="cf-age">Idade</Label>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card className="border-zinc-200">
        <CardHeader className="flex flex-row items-center justify-between pb-2">
          <CardTitle className="text-base font-semibold text-zinc-700">Perguntas</CardTitle>
          <Button type="button" size="sm" variant="ghost" onClick={addQuestion}
            className="text-zinc-500 hover:text-[var(--ink)] hover:bg-amber-50"
          >
            <Plus className="w-4 h-4 mr-1" />Adicionar Pergunta
          </Button>
        </CardHeader>
        <CardContent className="space-y-6">
          {questions.map((q, qi) => (
            <div key={qi} className="border border-zinc-200 rounded-lg p-4 space-y-3 bg-white">
              <div className="flex items-start gap-2">
                <div className="flex-1 space-y-1">
                  <Label>Pergunta {qi + 1}</Label>
                  <Input value={q.text} onChange={e => updateQuestion(qi, 'text', e.target.value)} placeholder="Texto da pergunta" />
                </div>
                {questions.length > 1 && (
                  <Button type="button" variant="ghost" size="sm" className="text-red-400 hover:text-red-600 hover:bg-red-50 mt-5" onClick={() => removeQuestion(qi)}>
                    <Trash2 className="w-4 h-4" />
                  </Button>
                )}
              </div>
              <div className="flex items-center gap-2">
                <Checkbox id={`req-${qi}`} checked={q.isRequired} onCheckedChange={v => updateQuestion(qi, 'isRequired', !!v)} />
                <Label htmlFor={`req-${qi}`}>Obrigatória</Label>
              </div>
              <div className="space-y-2">
                <Label className="text-xs text-zinc-400 uppercase tracking-wide">Opções</Label>
                {q.options.map((o, oi) => (
                  <div key={oi} className="flex gap-2">
                    <Input value={o.text} onChange={e => updateOption(qi, oi, e.target.value)} placeholder={`Opção ${oi + 1}`} />
                    {q.options.length > 2 && (
                      <Button type="button" variant="ghost" size="sm" className="text-red-400 hover:text-red-600 hover:bg-red-50" onClick={() => removeOption(qi, oi)}>
                        <Trash2 className="w-4 h-4" />
                      </Button>
                    )}
                  </div>
                ))}
                <Button type="button" variant="ghost" size="sm" onClick={() => addOption(qi)}
                  className="text-zinc-400 hover:text-[var(--ink)] hover:bg-amber-50"
                >
                  <Plus className="w-3 h-3 mr-1" />Adicionar Opção
                </Button>
              </div>
            </div>
          ))}
        </CardContent>
      </Card>

      <div className="flex gap-3">
        <Button type="submit" disabled={loading}>
          {loading ? 'Criando...' : 'Criar Pesquisa'}
        </Button>
        <Button type="button" variant="outline" onClick={() => router.push('/surveys')}>
          Cancelar
        </Button>
      </div>
    </form>
  )
}
