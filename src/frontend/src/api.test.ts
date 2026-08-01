import { describe, expect, it } from 'vitest'
import { normalizeApiBaseUrl } from './api'

describe('normalizeApiBaseUrl', () => {
  it('normalizes empty to empty', () => {
    expect(normalizeApiBaseUrl('')).toBe('')
    expect(normalizeApiBaseUrl('   ')).toBe('')
  })

  it('removes trailing slash', () => {
    expect(normalizeApiBaseUrl('https://example.com/')).toBe('https://example.com')
  })
})
