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
    <nav className="border-b bg-white">
      <div className="container mx-auto px-4 max-w-4xl flex items-center justify-between h-14">
        <div className="flex items-center gap-6">
          <Link href="/surveys" className="font-semibold text-indigo-600 text-lg">
            Online Survey
          </Link>
          <Link href="/explore" className="text-sm text-zinc-600 hover:text-indigo-600">
            Explorar
          </Link>
        </div>
        {!loading && (
          <div className="flex items-center gap-3">
            {user ? (
              <>
                <span className="text-sm text-zinc-500 hidden sm:block">{user.email}</span>
                <Button variant="outline" size="sm" onClick={handleSignOut}>
                  Logout
                </Button>
              </>
            ) : (
              <LinkButton href="/auth/login" size="sm" className="bg-indigo-600 hover:bg-indigo-700">
                Entrar
              </LinkButton>
            )}
          </div>
        )}
      </div>
    </nav>
  )
}
