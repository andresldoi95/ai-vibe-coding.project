/**
 * Ecuador Tax Utilities
 * Handles IVA calculations and tax-related operations
 */

export interface ITaxCalculation {
  subtotal: number
  subtotalIva12: number
  subtotalIva0: number
  subtotalNoIva: number
  iva: number
  total: number
  descuento: number
}

export interface ITaxRate {
  code: string
  name: string
  rate: number
  sriCode: string
}

export const TAX_RATES: ITaxRate[] = [
  { code: 'IVA_12', name: 'IVA 12%', rate: 12.0, sriCode: '2' },
  { code: 'IVA_0', name: 'IVA 0%', rate: 0.0, sriCode: '0' },
  { code: 'NO_IVA', name: 'No IVA', rate: 0.0, sriCode: '6' },
  { code: 'IVA_EXEMPT', name: 'IVA Exento', rate: 0.0, sriCode: '7' }
]

export function useEcuadorTax() {
  /**
   * Calculate IVA (12%) from a subtotal
   */
  const calculateIVA = (subtotal: number, rate: number = 12): number => {
    return Number((subtotal * (rate / 100)).toFixed(2))
  }

  /**
   * Calculate tax for invoice line items
   * @param items Array of line items with price, quantity, discount, and taxCode
   */
  const calculateInvoiceTaxes = (items: any[]): ITaxCalculation => {
    let subtotalIva12 = 0
    let subtotalIva0 = 0
    let subtotalNoIva = 0
    let totalDescuento = 0

    items.forEach((item) => {
      const itemSubtotal = item.quantity * item.price
      const itemDiscount = item.discount || 0
      const itemTotal = itemSubtotal - itemDiscount

      totalDescuento += itemDiscount

      // Categorize by tax code
      if (item.taxCode === 'IVA_12') {
        subtotalIva12 += itemTotal
      } else if (item.taxCode === 'IVA_0') {
        subtotalIva0 += itemTotal
      } else if (item.taxCode === 'NO_IVA' || item.taxCode === 'IVA_EXEMPT') {
        subtotalNoIva += itemTotal
      }
    })

    const subtotal = subtotalIva12 + subtotalIva0 + subtotalNoIva
    const iva = calculateIVA(subtotalIva12, 12)
    const total = subtotal + iva

    return {
      subtotal: Number(subtotal.toFixed(2)),
      subtotalIva12: Number(subtotalIva12.toFixed(2)),
      subtotalIva0: Number(subtotalIva0.toFixed(2)),
      subtotalNoIva: Number(subtotalNoIva.toFixed(2)),
      iva: Number(iva.toFixed(2)),
      total: Number(total.toFixed(2)),
      descuento: Number(totalDescuento.toFixed(2))
    }
  }

  /**
   * Get tax rate by code
   */
  const getTaxRate = (code: string): ITaxRate | undefined => {
    return TAX_RATES.find((tax) => tax.code === code)
  }

  /**
   * Get tax rate by SRI code
   */
  const getTaxRateBySriCode = (sriCode: string): ITaxRate | undefined => {
    return TAX_RATES.find((tax) => tax.sriCode === sriCode)
  }

  return {
    calculateIVA,
    calculateInvoiceTaxes,
    getTaxRate,
    getTaxRateBySriCode,
    TAX_RATES
  }
}
