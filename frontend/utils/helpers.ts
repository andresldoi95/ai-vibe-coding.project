/**
 * String utility functions
 */

export function truncate(text: string, maxLength: number): string {
  if (text.length <= maxLength)
    return text
  return `${text.substring(0, maxLength)}...`
}

export function capitalize(text: string): string {
  if (!text)
    return ''
  return text.charAt(0).toUpperCase() + text.slice(1).toLowerCase()
}

export function slugify(text: string): string {
  return text
    .toLowerCase()
    .trim()
    .replace(/[^\w\s-]/g, '')
    .replace(/[\s_-]+/g, '-')
    .replace(/^-+|-+$/g, '')
}

export function generateRandomString(length: number = 10): string {
  const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789'
  let result = ''
  for (let i = 0; i < length; i++) {
    result += chars.charAt(Math.floor(Math.random() * chars.length))
  }
  return result
}

/**
 * Number utility functions
 */

export function clamp(value: number, min: number, max: number): number {
  return Math.min(Math.max(value, min), max)
}

export function roundToDecimals(value: number, decimals: number = 2): number {
  return Math.round(value * 10 ** decimals) / 10 ** decimals
}

/**
 * Array utility functions
 */

export function unique<T>(array: T[]): T[] {
  return [...new Set(array)]
}

export function groupBy<T>(array: T[], key: keyof T): Record<string, T[]> {
  return array.reduce((result, item) => {
    const groupKey = String(item[key])
    if (!result[groupKey]) {
      result[groupKey] = []
    }
    result[groupKey].push(item)
    return result
  }, {} as Record<string, T[]>)
}

/**
 * Object utility functions
 */

export function omit<T extends object, K extends keyof T>(obj: T, keys: K[]): Omit<T, K> {
  const result = { ...obj }
  keys.forEach(key => delete result[key])
  return result
}

export function pick<T extends object, K extends keyof T>(obj: T, keys: K[]): Pick<T, K> {
  const result = {} as Pick<T, K>
  keys.forEach((key) => {
    if (key in obj) {
      result[key] = obj[key]
    }
  })
  return result
}
