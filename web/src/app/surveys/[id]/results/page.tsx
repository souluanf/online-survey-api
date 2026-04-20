'use client'
import { useEffect, useState } from 'react'
import { useParams } from 'next/navigation'
import { api } from '@/lib/api'
import { SurveyResultResponse } from '@/lib/types'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { LinkButton } from '@/components/ui/link-button'
import { Progress } from '@/components/ui/progress'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Skeleton } from '@/components/ui/skeleton'
import { ArrowLeft } from 'lucide-react'

function barColor(pct: number) {
  if (pct > 50) return 'bg-green-500'
  if (pct > 25) return 'bg-blue-500'
  if (pct > 10) return 'bg-yellow-400'
  return 'bg-zinc-300'
}

export default function ResultsPage() {
  const { id } = useParams<{ id: string }>()
  const [result, setResult] = useState<SurveyResultResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    api.responses.results(id)
      .then(setResult)
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false))
  }, [id])

  if (loading) return (
    <div className="space-y-4">
      <Skeleton className="h-8 w-64" />
      <Skeleton className="h-32 w-full" />
      <Skeleton className="h-32 w-full" />
    </div>
  )

  if (error) return <Alert variant="destructive"><AlertDescription>{error}</AlertDescription></Alert>
  if (!result) return null

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <LinkButton href="/surveys" variant="outline" size="sm">
          <ArrowLeft className="w-4 h-4 mr-1" />Voltar às Pesquisas
        </LinkButton>
      </div>

      <div>
        <h1 className="text-2xl font-bold">{result.surveyTitle}</h1>
        <p className="text-zinc-500 mt-1">{result.totalResponses} resposta{result.totalResponses !== 1 ? 's' : ''}</p>
      </div>

      {result.questions.map(q => (
        <Card key={q.questionId}>
          <CardHeader>
            <CardTitle className="text-base">{q.questionText}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {q.options.map(o => (
              <div key={o.optionId} className="space-y-1">
                <div className="flex justify-between text-sm">
                  <span>{o.optionText}</span>
                  <span className="text-zinc-500">{o.count} ({o.percentage.toFixed(1)}%)</span>
                </div>
                <div className="w-full bg-zinc-100 rounded-full h-2">
                  <div
                    className={`h-2 rounded-full transition-all ${barColor(o.percentage)}`}
                    style={{ width: `${o.percentage}%` }}
                  />
                </div>
              </div>
            ))}
          </CardContent>
        </Card>
      ))}

      {result.questions.length === 0 && (
        <Card>
          <CardContent className="py-10 text-center text-zinc-500">
            Nenhuma resposta registrada ainda.
          </CardContent>
        </Card>
      )}
    </div>
  )
}
