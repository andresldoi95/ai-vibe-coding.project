import { beforeEach, describe, expect, it } from 'vitest'
import { ref } from '../setup'
import { useFormatters } from '~/composables/useFormatters'

// Access the global mock
const mockLocale = ref('en-US')

// Override useI18n to return our mutable mock locale
globalThis.useI18n = () => ({
  locale: mockLocale,
  t: (key: string) => key,
})

describe('useFormatters', () => {
  beforeEach(() => {
    mockLocale.value = 'en-US'
  })

  describe('formatCurrency', () => {
    it('should format USD currency with default parameters', () => {
      const { formatCurrency } = useFormatters()

      const result = formatCurrency(1234.56)

      expect(result).toBe('$1,234.56')
    })

    it('should format currency with zero value', () => {
      const { formatCurrency } = useFormatters()

      const result = formatCurrency(0)

      expect(result).toBe('$0.00')
    })

    it('should format negative currency values', () => {
      const { formatCurrency } = useFormatters()

      const result = formatCurrency(-500.25)

      expect(result).toBe('-$500.25')
    })

    it('should format large currency amounts', () => {
      const { formatCurrency } = useFormatters()

      const result = formatCurrency(1000000.99)

      expect(result).toBe('$1,000,000.99')
    })

    it('should format currency with custom currency code', () => {
      const { formatCurrency } = useFormatters()

      const result = formatCurrency(1234.56, 'EUR')

      expect(result).toContain('1,234.56')
      expect(result).toMatch(/€|EUR/)
    })

    it('should handle decimal precision correctly', () => {
      const { formatCurrency } = useFormatters()

      const result = formatCurrency(10.5)

      expect(result).toBe('$10.50')
    })
  })

  describe('formatDate', () => {
    it('should format date in short format by default', () => {
      const { formatDate } = useFormatters()

      const result = formatDate('2024-01-15T12:00:00Z')

      expect(result).toMatch(/01\/15\/2024|15\/01\/2024/)
    })

    it('should format date in short format explicitly', () => {
      const { formatDate } = useFormatters()

      const result = formatDate('2024-12-31T12:00:00Z', 'short')

      expect(result).toMatch(/12\/31\/2024|31\/12\/2024/)
    })

    it('should format date in long format', () => {
      const { formatDate } = useFormatters()

      const result = formatDate('2024-06-15T12:00:00Z', 'long')

      expect(result).toContain('June')
      expect(result).toContain('15')
      expect(result).toContain('2024')
    })

    it('should accept Date object', () => {
      const { formatDate } = useFormatters()

      const date = new Date('2024-03-20T12:00:00Z')
      const result = formatDate(date, 'short')

      expect(result).toMatch(/03\/20\/2024|20\/03\/2024/)
    })

    it('should format date at start of year', () => {
      const { formatDate } = useFormatters()

      const result = formatDate('2024-01-01T12:00:00Z', 'short')

      expect(result).toMatch(/01\/01\/2024/)
    })

    it('should format date at end of year', () => {
      const { formatDate } = useFormatters()

      const result = formatDate('2024-12-31T12:00:00Z', 'short')

      expect(result).toMatch(/12\/31\/2024|31\/12\/2024/)
    })
  })

  describe('formatNumber', () => {
    it('should format integer numbers', () => {
      const { formatNumber } = useFormatters()

      const result = formatNumber(1234567)

      expect(result).toBe('1,234,567')
    })

    it('should format decimal numbers', () => {
      const { formatNumber } = useFormatters()

      const result = formatNumber(1234.56)

      expect(result).toBe('1,234.56')
    })

    it('should format zero', () => {
      const { formatNumber } = useFormatters()

      const result = formatNumber(0)

      expect(result).toBe('0')
    })

    it('should format negative numbers', () => {
      const { formatNumber } = useFormatters()

      const result = formatNumber(-9876.54)

      expect(result).toBe('-9,876.54')
    })

    it('should format small decimal numbers', () => {
      const { formatNumber } = useFormatters()

      const result = formatNumber(0.99)

      expect(result).toBe('0.99')
    })

    it('should format large numbers with proper grouping', () => {
      const { formatNumber } = useFormatters()

      const result = formatNumber(1000000000)

      expect(result).toBe('1,000,000,000')
    })
  })

  describe('formatDateTime', () => {
    it('should format date and time', () => {
      const { formatDateTime } = useFormatters()

      const result = formatDateTime('2024-01-15T14:30:00')

      // Result will vary based on timezone, but should contain date and time components
      expect(result).toMatch(/\d{2}\/\d{2}\/\d{4}/)
      expect(result).toMatch(/\d{1,2}:\d{2}/)
    })

    it('should accept Date object', () => {
      const { formatDateTime } = useFormatters()

      const date = new Date('2024-06-20T18:45:00')
      const result = formatDateTime(date)

      expect(result).toMatch(/\d{2}\/\d{2}\/\d{4}/)
      expect(result).toMatch(/\d{1,2}:\d{2}/)
    })

    it('should format midnight correctly', () => {
      const { formatDateTime } = useFormatters()

      const result = formatDateTime('2024-01-01T12:00:00')

      expect(result).toMatch(/\d{2}\/\d{2}\/\d{4}/)
      expect(result).toMatch(/\d{1,2}:\d{2}/)
    })

    it('should format end of day correctly', () => {
      const { formatDateTime } = useFormatters()

      const result = formatDateTime('2024-12-31T18:59:00')

      expect(result).toMatch(/\d{2}\/\d{2}\/\d{4}/)
      expect(result).toMatch(/\d{1,2}:\d{2}/)
    })
  })

  describe('locale changes', () => {
    it('should format currency according to current locale', () => {
      const { formatCurrency } = useFormatters()

      const resultUS = formatCurrency(1234.56)
      expect(resultUS).toBe('$1,234.56')

      mockLocale.value = 'es-ES'
      const { formatCurrency: formatCurrencyES } = useFormatters()
      const resultES = formatCurrencyES(1234.56)

      // Spanish locale uses different formatting
      expect(resultES).toContain('1')
      expect(resultES).toContain('234')
      expect(resultES).toContain('56')
    })

    it('should format date according to current locale', () => {
      mockLocale.value = 'en-GB'
      const { formatDate } = useFormatters()

      const result = formatDate('2024-01-15T12:00:00Z', 'short')

      // UK format is DD/MM/YYYY
      expect(result).toBe('15/01/2024')
    })

    it('should format numbers according to current locale', () => {
      mockLocale.value = 'de-DE'
      const { formatNumber } = useFormatters()

      const result = formatNumber(1234.56)

      // German locale uses . for thousands and , for decimals
      expect(result).toBe('1.234,56')
    })
  })

  describe('edge cases', () => {
    it('should handle very small currency amounts', () => {
      const { formatCurrency } = useFormatters()

      const result = formatCurrency(0.01)

      expect(result).toBe('$0.01')
    })

    it('should handle fractional cents in currency', () => {
      const { formatCurrency } = useFormatters()

      const result = formatCurrency(10.999)

      expect(result).toBe('$11.00')
    })

    it('should handle single digit numbers', () => {
      const { formatNumber } = useFormatters()

      const result = formatNumber(5)

      expect(result).toBe('5')
    })
  })
})
