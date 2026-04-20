'use client'
import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useAuth } from '@/lib/auth'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Alert, AlertDescription } from '@/components/ui/alert'

const schema = z.object({
  email: z.string().email('Email inválido'),
  password: z.string().min(6, 'Mínimo 6 caracteres'),
})

type FormData = z.infer<typeof schema>

export default function LoginPage() {
  const { user, signIn, signUp, signInWithGoogle } = useAuth()
  const router = useRouter()
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    if (user) router.push('/surveys')
  }, [user, router])

  const { register: registerField, handleSubmit, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  const onSignIn = async (data: FormData) => {
    setError(null)
    setLoading(true)
    try {
      await signIn(data.email, data.password)
      router.push('/surveys')
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erro ao entrar')
    } finally {
      setLoading(false)
    }
  }

  const onSignUp = async (data: FormData) => {
    setError(null)
    setLoading(true)
    try {
      await signUp(data.email, data.password)
      router.push('/surveys')
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erro ao criar conta')
    } finally {
      setLoading(false)
    }
  }

  const onGoogle = async () => {
    setError(null)
    setLoading(true)
    try {
      await signInWithGoogle()
      router.push('/surveys')
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erro ao entrar com Google')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="flex justify-center items-center min-h-[60vh]">
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle className="text-center text-indigo-600">Online Survey</CardTitle>
        </CardHeader>
        <CardContent>
          <Tabs defaultValue="signin">
            <TabsList className="w-full mb-4">
              <TabsTrigger value="signin" className="flex-1">Entrar</TabsTrigger>
              <TabsTrigger value="signup" className="flex-1">Criar conta</TabsTrigger>
            </TabsList>

            <TabsContent value="signin">
              <form onSubmit={handleSubmit(onSignIn)} className="space-y-4">
                <div className="space-y-1">
                  <Label htmlFor="email-in">Email</Label>
                  <Input id="email-in" type="email" {...registerField('email')} />
                  {errors.email && <p className="text-xs text-red-500">{errors.email.message}</p>}
                </div>
                <div className="space-y-1">
                  <Label htmlFor="pass-in">Senha</Label>
                  <Input id="pass-in" type="password" {...registerField('password')} />
                  {errors.password && <p className="text-xs text-red-500">{errors.password.message}</p>}
                </div>
                {error && <Alert variant="destructive"><AlertDescription>{error}</AlertDescription></Alert>}
                <Button type="submit" className="w-full bg-indigo-600 hover:bg-indigo-700" disabled={loading}>
                  Entrar
                </Button>
              </form>
            </TabsContent>

            <TabsContent value="signup">
              <form onSubmit={handleSubmit(onSignUp)} className="space-y-4">
                <div className="space-y-1">
                  <Label htmlFor="email-up">Email</Label>
                  <Input id="email-up" type="email" {...registerField('email')} />
                  {errors.email && <p className="text-xs text-red-500">{errors.email.message}</p>}
                </div>
                <div className="space-y-1">
                  <Label htmlFor="pass-up">Senha</Label>
                  <Input id="pass-up" type="password" {...registerField('password')} />
                  {errors.password && <p className="text-xs text-red-500">{errors.password.message}</p>}
                </div>
                {error && <Alert variant="destructive"><AlertDescription>{error}</AlertDescription></Alert>}
                <Button type="submit" className="w-full bg-indigo-600 hover:bg-indigo-700" disabled={loading}>
                  Criar conta
                </Button>
              </form>
            </TabsContent>
          </Tabs>

          <div className="mt-4">
            <div className="relative flex items-center gap-2 my-4">
              <div className="flex-1 h-px bg-zinc-200" />
              <span className="text-xs text-zinc-400">ou</span>
              <div className="flex-1 h-px bg-zinc-200" />
            </div>
            {error && <Alert variant="destructive" className="mb-2"><AlertDescription>{error}</AlertDescription></Alert>}
            <Button variant="outline" className="w-full" onClick={onGoogle} disabled={loading}>
              <svg className="w-4 h-4 mr-2" viewBox="0 0 24 24">
                <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
                <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
                <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"/>
                <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/>
              </svg>
              Entrar com Google
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
