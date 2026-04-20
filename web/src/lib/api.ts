const API_URL = process.env.NEXT_PUBLIC_API_URL!

const ERROR_TRANSLATIONS: Record<string, string> = {
  'Only draft surveys can be updated.': 'Apenas pesquisas em rascunho podem ser editadas.',
  'Cannot delete an active survey. Close it first.': 'Não é possível excluir uma pesquisa ativa. Encerre-a primeiro.',
  'Survey not found.': 'Pesquisa não encontrada.',
  'Survey is not active.': 'Esta pesquisa não está ativa.',
  'Invalid access code.': 'Código de acesso inválido.',
  'Access code expired.': 'Código de acesso expirado.',
  'Access code already used.': 'Código de acesso já utilizado.',
  'Email is required.': 'O email é obrigatório.',
  'Title is required.': 'O título é obrigatório.',
  'Survey must have at least one question.': 'A pesquisa precisa ter pelo menos uma pergunta.',
  'Question must have at least two options.': 'Cada pergunta precisa ter pelo menos duas opções.',
  'Unauthorized': 'Você não tem permissão para executar esta ação.',
}

function extractMessage(status: number, text: string): string {
  try {
    const data = JSON.parse(text)
    const raw = data?.detail || data?.title || data?.message
    if (raw && typeof raw === 'string') return ERROR_TRANSLATIONS[raw] ?? raw
    if (data?.errors && typeof data.errors === 'object') {
      const msgs = Object.values(data.errors).flat()
      if (msgs.length) return msgs.join(' ')
    }
  } catch { /* not JSON */ }
  if (status === 401) return 'Sessão expirada. Faça login novamente.'
  if (status === 403) return 'Você não tem permissão para executar esta ação.'
  if (status === 404) return 'Recurso não encontrado.'
  if (status >= 500) return 'Erro interno no servidor. Tente novamente mais tarde.'
  return text || `Erro ${status}`
}

async function fetchWithAuth(path: string, token: string | null, options: RequestInit = {}) {
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...options.headers,
  }
  const res = await fetch(`${API_URL}${path}`, { ...options, headers })
  if (!res.ok) {
    const text = await res.text()
    throw new Error(extractMessage(res.status, text))
  }
  if (res.status === 204) return null
  return res.json()
}

export const api = {
  surveys: {
    list: (token: string, page = 1, pageSize = 10) =>
      fetchWithAuth(`/api/surveys?page=${page}&pageSize=${pageSize}`, token),
    get: (id: string, token?: string | null) =>
      fetchWithAuth(`/api/surveys/${id}`, token ?? null),
    create: (token: string, data: unknown) =>
      fetchWithAuth('/api/surveys', token, { method: 'POST', body: JSON.stringify(data) }),
    update: (token: string, id: string, data: unknown) =>
      fetchWithAuth(`/api/surveys/${id}`, token, { method: 'PUT', body: JSON.stringify(data) }),
    activate: (token: string, id: string, data: unknown) =>
      fetchWithAuth(`/api/surveys/${id}/activate`, token, { method: 'POST', body: JSON.stringify(data) }),
    close: (token: string, id: string) =>
      fetchWithAuth(`/api/surveys/${id}/close`, token, { method: 'POST', body: '{}' }),
    delete: (token: string, id: string) =>
      fetchWithAuth(`/api/surveys/${id}`, token, { method: 'DELETE' }),
    active: () =>
      fetchWithAuth('/api/surveys/active', null),
    requestCode: (id: string, email: string) =>
      fetchWithAuth(`/api/surveys/${id}/access/request`, null, { method: 'POST', body: JSON.stringify({ email }) }),
    verifyCode: (id: string, email: string, code: string) =>
      fetchWithAuth(`/api/surveys/${id}/access/verify`, null, { method: 'POST', body: JSON.stringify({ email, code }) }),
  },
  responses: {
    submit: (data: unknown) =>
      fetchWithAuth('/api/responses', null, { method: 'POST', body: JSON.stringify(data) }),
    results: (surveyId: string) =>
      fetchWithAuth(`/api/responses/surveys/${surveyId}/results`, null),
  },
}
