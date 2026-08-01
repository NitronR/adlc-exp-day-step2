import React from 'react'
import { getAudit } from '../api'
import type { ConversionResponse } from '../api'

export function AuditLookup() {
  const [auditId, setAuditId] = React.useState('')
  const [loading, setLoading] = React.useState(false)
  const [error, setError] = React.useState<{ title: string; detail?: string; status?: number } | null>(null)
  const [result, setResult] = React.useState<ConversionResponse | null>(null)

  async function onLookup(e: React.FormEvent) {
    e.preventDefault()
    setError(null)
    setResult(null)

    const trimmed = auditId.trim()
    if (!trimmed) {
      setError({ title: 'Missing audit ID', detail: 'Enter an audit record identifier returned by the conversion response.' })
      return
    }

    setLoading(true)
    try {
      const response = await getAudit(trimmed)
      setResult(response)
    } catch (err: any) {
      setError({ title: err.title ?? 'Request failed', detail: err.detail, status: err.status })
    } finally {
      setLoading(false)
    }
  }

  return (
    <form onSubmit={onLookup}>
      <label className="label">
        Audit ID
        <input value={auditId} onChange={(e) => setAuditId(e.target.value)} placeholder="e.g. 3d8a7c..." />
      </label>

      <div className="btnRow">
        <button className="btn" type="submit" disabled={loading}>
          {loading ? 'Looking up...' : 'Lookup'}
        </button>
      </div>

      {error ? (
        <div className="status statusErr" role="status" aria-live="polite">
          <div style={{ fontWeight: 800, marginBottom: 6 }}>{error.title}</div>
          {error.detail ? <div style={{ color: 'var(--muted)', lineHeight: 1.35 }}>{error.detail}</div> : null}
        </div>
      ) : null}

      {result ? (
        <div className="status statusOk" role="status" aria-live="polite">
          <div style={{ fontWeight: 800, marginBottom: 6 }}>Audit record found</div>
          <div className="kv">
            <div className="k">Audit ID</div>
            <div className="v">{result.auditId}</div>
            <div className="k">Stored result</div>
            <div className="v">
              {result.sourceAmount} {result.from} → {result.convertedAmount} {result.to}
            </div>
            <div className="k">Rate applied</div>
            <div className="v">{result.rateApplied}</div>
            <div className="k">Provider date</div>
            <div className="v">{result.providerDate ?? '—'}</div>
            <div className="k">Executed at (UTC)</div>
            <div className="v">{new Date(result.executedAtUtc).toISOString()}</div>
          </div>
          <div className="hint">Retrieval reads the stored Cosmos DB record; it does not call the external provider again.</div>
        </div>
      ) : null}
    </form>
  )
}
