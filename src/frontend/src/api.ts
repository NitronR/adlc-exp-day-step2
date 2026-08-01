export function normalizeApiBaseUrl(value: string): string {
  const trimmed = value.trim()
  if (!trimmed) return ''
  return trimmed.endsWith('/') ? trimmed.slice(0, -1) : trimmed
}

export const apiBaseUrl = normalizeApiBaseUrl((window as any).__VITE_API_URL__ ?? '')

export type ProblemDetails = {
  title?: string
  status?: number
  detail?: string
}

export type ConversionResponse = {
  auditId: string
  from: string
  to: string
  sourceAmount: number
  rateApplied: number
  convertedAmount: number
  providerDate?: string
  executedAtUtc: string
}

function json<T>(res: Response): Promise<T> {
  return res.json() as Promise<T>
}

export async function convertCurrency(payload: {
  amount: number
  from: string
  to: string
}): Promise<ConversionResponse> {
  const res = await fetch(`${apiBaseUrl}/api/conversions`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  })

  if (!res.ok) {
    const body = (await res.json().catch(() => null)) as ProblemDetails | null
    throw {
      status: res.status,
      title: body?.title ?? 'Request failed',
      detail: body?.detail
    }
  }

  return json<ConversionResponse>(res)
}

export async function getAudit(auditId: string): Promise<ConversionResponse> {
  const res = await fetch(`${apiBaseUrl}/api/audits/${encodeURIComponent(auditId)}`)

  if (!res.ok) {
    const body = (await res.json().catch(() => null)) as ProblemDetails | null
    throw {
      status: res.status,
      title: body?.title ?? 'Request failed',
      detail: body?.detail
    }
  }

  return json<ConversionResponse>(res)
}
