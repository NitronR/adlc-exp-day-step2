import React from 'react'
import { convertCurrency } from '../api'
import type { ConversionResponse } from '../api'

export function ConversionForm() {
  const [amount, setAmount] = React.useState('100.00')
  const [from, setFrom] = React.useState('USD')
  const [to, setTo] = React.useState('EUR')
  const [loading, setLoading] = React.useState(false)
  const [error, setError] = React.useState<{ title: string; detail?: string; status?: number } | null>(null)
  const [result, setResult] = React.useState<ConversionResponse | null>(null)

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError(null)
    setResult(null)

    const parsed = Number(amount)
    if (!Number.isFinite(parsed) || parsed <= 0) {
      setError({ title: 'Invalid amount', detail: 'Amount must be a positive number.' })
      return
    }

    setLoading(true)
    try {
      const response = await convertCurrency({ amount: parsed, from, to })
      setResult(response)
    } catch (err: any) {
      setError({ title: err.title ?? 'Request failed', detail: err.detail, status: err.status })
    } finally {
      setLoading(false)
    }
  }

  return (
    <form onSubmit={onSubmit}>
      <div className="formGrid">
        <label className="label">
          Amount
          <input value={amount} onChange={(e) => setAmount(e.target.value)} inputMode="decimal" />
        </label>
        <label className="label">
          From
          <input value={from} onChange={(e) => setFrom(e.target.value)} maxLength={3} />
        </label>
        <label className="label">
          To
          <input value={to} onChange={(e) => setTo(e.target.value)} maxLength={3} />
        </label>
      </div>

      <div className="btnRow">
        <button className="btn" type="submit" disabled={loading}>
          {loading ? 'Converting...' : 'Convert'}
        </button>
        <button
          className="btnSecondary"
          type="button"
          disabled={loading}
          onClick={() => {
            setAmount('100.00')
            setFrom('USD')
            setTo('EUR')
            setError(null)
            setResult(null)
          }}>
          Reset
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
          <div style={{ fontWeight: 800, marginBottom: 6 }}>Conversion complete</div>
          <div className="kv">
            <div className="k">Audit ID</div>
            <div className="v">{result.auditId}</div>
            <div className="k">Result</div>
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
          <div className="hint">
            Store the audit ID. Use it in the Audit Lookup panel to retrieve the same stored result later.
          </div>
        </div>
      ) : null}
    </form>
  )
}
