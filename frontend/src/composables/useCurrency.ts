/**
 * Currency Utilities for Ecuador (USD)
 */
import numeral from 'numeral'

// Configure numeral for Spanish (Ecuador)
numeral.register('locale', 'es-ec', {
  delimiters: {
    thousands: '.',
    decimal: ','
  },
  abbreviations: {
    thousand: 'k',
    million: 'M',
    billion: 'B',
    trillion: 'T'
  },
  ordinal: function () {
    return 'º'
  },
  currency: {
    symbol: '$'
  }
})

numeral.locale('es-ec')

export function useCurrency() {
  /**
   * Format amount as USD currency (Ecuador format)
   * Example: 1234.56 -> $1.234,56
   */
  const formatCurrency = (amount: number | string): string => {
    const num = typeof amount === 'string' ? parseFloat(amount) : amount
    if (isNaN(num)) return '$0,00'
    return numeral(num).format('$0,0.00')
  }

  /**
   * Format amount without currency symbol
   * Example: 1234.56 -> 1.234,56
   */
  const formatNumber = (amount: number | string, decimals: number = 2): string => {
    const num = typeof amount === 'string' ? parseFloat(amount) : amount
    if (isNaN(num)) return '0,00'
    const format = decimals === 0 ? '0,0' : `0,0.${'0'.repeat(decimals)}`
    return numeral(num).format(format)
  }

  /**
   * Parse formatted currency string to number
   * Example: "$1.234,56" -> 1234.56
   */
  const parseCurrency = (formatted: string): number => {
    const cleaned = formatted.replace(/[$\s]/g, '').replace(/\./g, '').replace(',', '.')
    return parseFloat(cleaned) || 0
  }

  /**
   * Convert number to words in Spanish (for invoices)
   * Example: 1234.56 -> "MIL DOSCIENTOS TREINTA Y CUATRO CON 56/100 DÓLARES"
   */
  const numberToWords = (amount: number): string => {
    const units = ['', 'UNO', 'DOS', 'TRES', 'CUATRO', 'CINCO', 'SEIS', 'SIETE', 'OCHO', 'NUEVE']
    const teens = ['DIEZ', 'ONCE', 'DOCE', 'TRECE', 'CATORCE', 'QUINCE', 'DIECISÉIS', 'DIECISIETE', 'DIECIOCHO', 'DIECINUEVE']
    const tens = ['', '', 'VEINTE', 'TREINTA', 'CUARENTA', 'CINCUENTA', 'SESENTA', 'SETENTA', 'OCHENTA', 'NOVENTA']
    const hundreds = ['', 'CIENTO', 'DOSCIENTOS', 'TRESCIENTOS', 'CUATROCIENTOS', 'QUINIENTOS', 'SEISCIENTOS', 'SETECIENTOS', 'OCHOCIENTOS', 'NOVECIENTOS']

    const convertGroup = (n: number): string => {
      if (n === 0) return ''
      if (n < 10) return units[n]
      if (n < 20) return teens[n - 10]
      if (n < 100) {
        const ten = Math.floor(n / 10)
        const unit = n % 10
        if (n === 20) return 'VEINTE'
        if (n < 30) return 'VEINTI' + units[unit]
        return tens[ten] + (unit ? ' Y ' + units[unit] : '')
      }
      const hundred = Math.floor(n / 100)
      const rest = n % 100
      const hundredStr = n === 100 ? 'CIEN' : hundreds[hundred]
      return hundredStr + (rest ? ' ' + convertGroup(rest) : '')
    }

    const [intPart, decPart] = amount.toFixed(2).split('.')
    const intNum = parseInt(intPart)
    
    let result = ''
    
    if (intNum === 0) {
      result = 'CERO'
    } else if (intNum < 1000) {
      result = convertGroup(intNum)
    } else if (intNum < 1000000) {
      const thousands = Math.floor(intNum / 1000)
      const rest = intNum % 1000
      result = (thousands === 1 ? 'MIL' : convertGroup(thousands) + ' MIL') + (rest ? ' ' + convertGroup(rest) : '')
    } else {
      const millions = Math.floor(intNum / 1000000)
      const rest = intNum % 1000000
      result = (millions === 1 ? 'UN MILLÓN' : convertGroup(millions) + ' MILLONES')
      if (rest >= 1000) {
        const thousands = Math.floor(rest / 1000)
        const hundreds = rest % 1000
        result += ' ' + (thousands === 1 ? 'MIL' : convertGroup(thousands) + ' MIL')
        if (hundreds) result += ' ' + convertGroup(hundreds)
      } else if (rest) {
        result += ' ' + convertGroup(rest)
      }
    }

    return `${result} CON ${decPart}/100 DÓLARES`
  }

  return {
    formatCurrency,
    formatNumber,
    parseCurrency,
    numberToWords
  }
}
