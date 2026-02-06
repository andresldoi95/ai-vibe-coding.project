/**
 * Utility functions for validation
 */

export function isValidEmail(email: string): boolean {
  const emailRegex = /^[^\s@]+@[^\s@][^\s.@]*\.[^\s@]+$/
  return emailRegex.test(email)
}

export function isValidUrl(url: string): boolean {
  try {
    const _validUrl = new URL(url)
    return true
  }
  catch {
    return false
  }
}

export function isValidPhone(phone: string): boolean {
  const phoneRegex = /^[\d\s\-+()]+$/
  return phoneRegex.test(phone) && phone.replace(/\D/g, '').length >= 10
}

export function isValidSku(sku: string): boolean {
  // SKU should be alphanumeric with optional dashes/underscores
  const skuRegex = /^[\w\-]+$/
  return skuRegex.test(sku) && sku.length >= 3
}
