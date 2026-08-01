import React from 'react'
import { ConversionForm } from './components/ConversionForm'
import { AuditLookup } from './components/AuditLookup'

export default function App() {
  return (
    <div className="page">
      <header className="header">
        <div className="titleBlock">
          <h1 className="title">Currency Conversion</h1>
          <p className="subtitle">Instant conversion with an audit trail you can retrieve on demand.</p>
        </div>
      </header>

      <main className="main">
        <section className="card">
          <h2 className="cardTitle">New Conversion</h2>
          <ConversionForm />
        </section>

        <section className="card">
          <h2 className="cardTitle">Audit Lookup</h2>
          <AuditLookup />
        </section>
      </main>
    </div>
  )
}
