import { beforeEach, describe, expect, it } from 'vitest'
import { mockLocale } from '../setup'
import { useFormatters } from '~/composables/useFormatters'

describe('useFormatters', () => {
  beforeEach(() => {
    // Reset locale to default before each test
    mockLocale.value = 'en-US'
  })

  describe('formatCurrency', () => {
    it('should format currency in USD for en-US locale', () => {
      const { formatCurrency } = useFormatters()
      const result = formatCurrency(1234.56, 'USD')
      expect(result).toBe('$1,234.56')
    })

    it('should format currency in EUR for en-US locale', () => {
      const { formatCurrency } = useFormatters()
      const result = formatCurrency(1234.56, 'EUR')
      expect(result).toBe('€1,234.56')
    })

    it('should format currency in EUR for es-ES locale', () => {
      mockLocale.value = 'es-ES'
      const { formatCurrency } = useFormatters()
      const result = formatCurrency(1234.56, 'EUR')
      // es-ES uses non-breaking space (\u00a0) before €
      expect(result).toBe('1234,56\u00A0€')
    })

    it('should format currency in EUR for fr-FR locale', () => {
      mockLocale.value = 'fr-FR'
      const { formatCurrency } = useFormatters()
      const result = formatCurrency(1234.56, 'EUR')
      // fr-FR uses narrow non-breaking space (\u202f) as separator
      expect(result).toBe('1\u202F234,56\u00A0€')
    })

    it('should format negative currency amounts', () => {
      const { formatCurrency } = useFormatters()
      const result = formatCurrency(-500.25, 'USD')
      expect(result).toBe('-$500.25')
    })

    it('should format zero as currency', () => {
      const { formatCurrency } = useFormatters()
      const result = formatCurrency(0, 'USD')
      expect(result).toBe('$0.00')
    })

    it('should format large currency amounts', () => {
      const { formatCurrency } = useFormatters()
      const result = formatCurrency(9876543.21, 'USD')
      expect(result).toBe('$9,876,543.21')
    })

    it('should use USD as default currency', () => {
      const { formatCurrency } = useFormatters()
      const result = formatCurrency(100)
      expect(result).toBe('$100.00')
    })
  })

  describe('formatDate', () => {
    it('should format date in short format for en-US locale', () => {
      const { formatDate } = useFormatters()
      const date = new Date('2024-03-15T10:30:00Z')
      const result = formatDate(date, 'short')
      expect(result).toBe('03/15/2024')
    })

    it('should format date in long format for en-US locale', () => {
      const { formatDate } = useFormatters()
      const date = new Date('2024-03-15T10:30:00Z')
      const result = formatDate(date, 'long')
      expect(result).toBe('March 15, 2024')
    })

    it('should format date in short format for es-ES locale', () => {
      mockLocale.value = 'es-ES'
      const { formatDate } = useFormatters()
      const date = new Date('2024-03-15T10:30:00Z')
      const result = formatDate(date, 'short')
      expect(result).toBe('15/03/2024')
    })

    it('should format date in long format for fr-FR locale', () => {
      mockLocale.value = 'fr-FR'
      const { formatDate } = useFormatters()
      const date = new Date('2024-03-15T10:30:00Z')
      const result = formatDate(date, 'long')
      expect(result).toBe('15 mars 2024')
    })

    it('should format string date input', () => {
      const { formatDate } = useFormatters()
      const result = formatDate('2024-01-01', 'short')
      expect(result).toBe('01/01/2024')
    })

    it('should use short format as default', () => {
      const { formatDate } = useFormatters()
      const date = new Date('2024-06-20T12:00:00Z')
      const result = formatDate(date)
      expect(result).toBe('06/20/2024')
    })

    it('should handle invalid date gracefully', () => {
      const { formatDate } = useFormatters()
      // Invalid dates throw RangeError when formatting
      expect(() => formatDate('invalid-date', 'short')).toThrow(RangeError)
    })
  })

  describe('formatNumber', () => {
    it('should format number for en-US locale', () => {
      const { formatNumber } = useFormatters()
      const result = formatNumber(1234567.89)
      expect(result).toBe('1,234,567.89')
    })

    it('should format number for es-ES locale', () => {
      mockLocale.value = 'es-ES'
      const { formatNumber } = useFormatters()
      const result = formatNumber(1234567.89)
      expect(result).toBe('1.234.567,89')
    })

    it('should format number for fr-FR locale', () => {
      mockLocale.value = 'fr-FR'
      const { formatNumber } = useFormatters()
      const result = formatNumber(1234567.89)
      // fr-FR uses narrow non-breaking space (\u202f) as thousands separator
      expect(result).toBe('1\u202F234\u202F567,89')
    })

    it('should format negative numbers', () => {
      const { formatNumber } = useFormatters()
      const result = formatNumber(-9876.54)
      expect(result).toBe('-9,876.54')
    })

    it('should format zero', () => {
      const { formatNumber } = useFormatters()
      const result = formatNumber(0)
      expect(result).toBe('0')
    })

    it('should format very large numbers', () => {
      const { formatNumber } = useFormatters()
      const result = formatNumber(999999999999.99)
      expect(result).toBe('999,999,999,999.99')
    })

    it('should format integers without decimals', () => {
      const { formatNumber } = useFormatters()
      const result = formatNumber(1000)
      expect(result).toBe('1,000')
    })
  })

  describe('formatDateTime', () => {
    it('should format date and time for en-US locale', () => {
      const { formatDateTime } = useFormatters()
      const date = new Date('2024-03-15T10:30:00Z')
      const result = formatDateTime(date)
      // Note: Result may vary based on timezone, so we check for expected patterns
      expect(result).toMatch(/03\/15\/2024/)
      expect(result).toMatch(/\d{2}:\d{2}/)
    })

    it('should format date and time for es-ES locale', () => {
      mockLocale.value = 'es-ES'
      const { formatDateTime } = useFormatters()
      const date = new Date('2024-03-15T14:45:00Z')
      const result = formatDateTime(date)
      expect(result).toMatch(/15\/03\/2024/)
      expect(result).toMatch(/\d{2}:\d{2}/)
    })

    it('should format date and time for fr-FR locale', () => {
      mockLocale.value = 'fr-FR'
      const { formatDateTime } = useFormatters()
      const date = new Date('2024-03-15T18:20:00Z')
      const result = formatDateTime(date)
      expect(result).toMatch(/15\/03\/2024/)
      expect(result).toMatch(/\d{2}:\d{2}/)
    })

    it('should format string date input with time', () => {
      const { formatDateTime } = useFormatters()
      const result = formatDateTime('2024-12-31T23:59:00Z')
      expect(result).toMatch(/12\/31\/2024/)
      expect(result).toMatch(/\d{2}:\d{2}/)
    })

    it('should handle invalid date gracefully', () => {
      const { formatDateTime } = useFormatters()
      // Invalid dates throw RangeError when formatting
      expect(() => formatDateTime('invalid-date-time')).toThrow(RangeError)
    })
  })

  describe('locale reactivity', () => {
    it('should update formatting when locale changes', () => {
      const { formatCurrency, formatNumber } = useFormatters()

      // Format in en-US
      let currencyResult = formatCurrency(1234.56, 'EUR')
      let numberResult = formatNumber(1234.56)
      expect(currencyResult).toBe('€1,234.56')
      expect(numberResult).toBe('1,234.56')

      // Change locale to es-ES
      mockLocale.value = 'es-ES'

      // Format in es-ES (uses non-breaking space \u00a0)
      currencyResult = formatCurrency(1234.56, 'EUR')
      numberResult = formatNumber(1234.56)
      expect(currencyResult).toBe('1234,56\u00A0€')
      expect(numberResult).toBe('1234,56')
    })
  })
})
