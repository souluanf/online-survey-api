'use client'
import Link from 'next/link'
import { useAuth } from '@/lib/auth'
import { Button } from '@/components/ui/button'
import { LinkButton } from '@/components/ui/link-button'
import { useRouter } from 'next/navigation'

export function Navbar() {
  const { user, loading, signOut } = useAuth()
  const router = useRouter()

  const handleSignOut = async () => {
    await signOut()
    router.push('/auth/login')
  }

  return (
    <nav className="border-b border-zinc-200 bg-[var(--paper)]">
      <div className="container mx-auto px-4 max-w-4xl flex items-center justify-between h-14">
        <div className="flex items-center gap-6">
          <Link
            href="/"
            className="font-heading text-lg font-semibold text-[var(--ink)] tracking-tight"
          >
            Online Survey
          </Link>
          <Link
            href="/surveys"
            className="text-sm text-zinc-500 hover:text-[var(--ink)] transition-colors"
          >
            Minhas pesquisas
          </Link>
        </div>
        {!loading && (
          <div className="flex items-center gap-3">
            {user ? (
              <>
                <span className="text-sm text-zinc-400 hidden sm:block">{user.email}</span>
                <Button variant="outline" size="sm" onClick={handleSignOut}>
                  Logout
                </Button>
              </>
            ) : (
              <LinkButton href="/auth/login" size="sm">
                Entrar
              </LinkButton>
            )}
          </div>
        )}
      </div>
    </nav>
  )
}
