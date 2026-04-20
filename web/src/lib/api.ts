const API_URL = process.env.NEXT_PUBLIC_API_URL!

async function fetchWithAuth(path: string, token: string | null, options: RequestInit = {}) {
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...options.headers,
  }
  const res = await fetch(`${API_URL}${path}`, { ...options, headers })
  if (!res.ok) {
    const text = await res.text()
    throw new Error(`${res.status}: ${text}`)
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
