import type { Metadata } from 'next'
import { Inter, Playfair_Display } from 'next/font/google'
import './globals.css'
import { Providers } from './providers'
import { ConditionalNavbar } from '@/components/conditional-navbar'
import { ConditionalMain } from '@/components/conditional-main'

const inter = Inter({ subsets: ['latin'], variable: '--font-inter' })
const playfair = Playfair_Display({
  subsets: ['latin'],
  weight: ['600', '700'],
  style: ['normal', 'italic'],
  variable: '--font-playfair',
})

export const metadata: Metadata = {
  title: 'Online Survey — Pesquisas que as pessoas realmente querem responder',
  description: 'Crie pesquisas com controle de acesso avançado: público, por código de email ou login obrigatório. Resultados em tempo real, exportação CSV e muito mais.',
}

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="pt-BR">
      <body className={`${inter.variable} ${playfair.variable} ${inter.className}`}>
        <Providers>
          <ConditionalNavbar />
          <ConditionalMain>{children}</ConditionalMain>
        </Providers>
      </body>
    </html>
  )
}
