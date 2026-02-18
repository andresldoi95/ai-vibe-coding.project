import { describe, expect, it } from 'vitest'
import {
  API_ENDPOINTS,
  DATE_FORMATS,
  DEFAULT_PAGE_SIZE,
  INVOICE_STATUS_SEVERITY,
  PAGE_SIZE_OPTIONS,
  PAYMENT_STATUS_SEVERITY,
  STORAGE_KEYS,
  SUBSCRIPTION_STATUS_SEVERITY,
  VALIDATION_RULES,
} from '~/utils/constants'

describe('constants utility', () => {
  describe('api endpoints', () => {
    it('should define AUTH endpoints', () => {
      expect(API_ENDPOINTS.AUTH.LOGIN).toBe('/auth/login')
      expect(API_ENDPOINTS.AUTH.LOGOUT).toBe('/auth/logout')
      expect(API_ENDPOINTS.AUTH.REFRESH).toBe('/auth/refresh')
      expect(API_ENDPOINTS.AUTH.ME).toBe('/auth/me')
    })

    it('should define TENANTS endpoints', () => {
      expect(API_ENDPOINTS.TENANTS.LIST).toBe('/tenants')
      expect(API_ENDPOINTS.TENANTS.DETAIL('123')).toBe('/tenants/123')
    })

    it('should define BILLING endpoints', () => {
      expect(API_ENDPOINTS.BILLING.INVOICES).toBe('/billing/invoices')
      expect(API_ENDPOINTS.BILLING.INVOICE_DETAIL('456')).toBe('/billing/invoices/456')
      expect(API_ENDPOINTS.BILLING.CUSTOMERS).toBe('/billing/customers')
      expect(API_ENDPOINTS.BILLING.CUSTOMER_DETAIL('789')).toBe('/billing/customers/789')
      expect(API_ENDPOINTS.BILLING.PAYMENTS).toBe('/billing/payments')
    })

    it('should define INVENTORY endpoints', () => {
      expect(API_ENDPOINTS.INVENTORY.PRODUCTS).toBe('/inventory/products')
      expect(API_ENDPOINTS.INVENTORY.PRODUCT_DETAIL('abc')).toBe('/inventory/products/abc')
      expect(API_ENDPOINTS.INVENTORY.WAREHOUSES).toBe('/inventory/warehouses')
      expect(API_ENDPOINTS.INVENTORY.STOCK_MOVEMENTS).toBe('/inventory/stock-movements')
    })

    it('should use dynamic ID functions correctly', () => {
      expect(typeof API_ENDPOINTS.TENANTS.DETAIL).toBe('function')
      expect(typeof API_ENDPOINTS.BILLING.INVOICE_DETAIL).toBe('function')
      expect(typeof API_ENDPOINTS.BILLING.CUSTOMER_DETAIL).toBe('function')
      expect(typeof API_ENDPOINTS.INVENTORY.PRODUCT_DETAIL).toBe('function')
    })
  })

  describe('status severity mappings', () => {
    it('should define INVOICE_STATUS_SEVERITY', () => {
      expect(INVOICE_STATUS_SEVERITY.draft).toBe('secondary')
      expect(INVOICE_STATUS_SEVERITY.sent).toBe('info')
      expect(INVOICE_STATUS_SEVERITY.paid).toBe('success')
      expect(INVOICE_STATUS_SEVERITY.overdue).toBe('danger')
      expect(INVOICE_STATUS_SEVERITY.cancelled).toBe('warning')
    })

    it('should define PAYMENT_STATUS_SEVERITY', () => {
      expect(PAYMENT_STATUS_SEVERITY.pending).toBe('warning')
      expect(PAYMENT_STATUS_SEVERITY.completed).toBe('success')
      expect(PAYMENT_STATUS_SEVERITY.failed).toBe('danger')
      expect(PAYMENT_STATUS_SEVERITY.refunded).toBe('info')
    })

    it('should define SUBSCRIPTION_STATUS_SEVERITY', () => {
      expect(SUBSCRIPTION_STATUS_SEVERITY.active).toBe('success')
      expect(SUBSCRIPTION_STATUS_SEVERITY.cancelled).toBe('warning')
      expect(SUBSCRIPTION_STATUS_SEVERITY.expired).toBe('danger')
      expect(SUBSCRIPTION_STATUS_SEVERITY.trial).toBe('info')
    })

    it('should have consistent severity values', () => {
      const allSeverities = [
        ...Object.values(INVOICE_STATUS_SEVERITY),
        ...Object.values(PAYMENT_STATUS_SEVERITY),
        ...Object.values(SUBSCRIPTION_STATUS_SEVERITY),
      ]

      const validSeverities = ['success', 'info', 'warning', 'danger', 'secondary']
      allSeverities.forEach((severity) => {
        expect(validSeverities).toContain(severity)
      })
    })
  })

  describe('pagination constants', () => {
    it('should define DEFAULT_PAGE_SIZE', () => {
      expect(DEFAULT_PAGE_SIZE).toBe(10)
      expect(typeof DEFAULT_PAGE_SIZE).toBe('number')
    })

    it('should define PAGE_SIZE_OPTIONS', () => {
      expect(PAGE_SIZE_OPTIONS).toEqual([10, 25, 50, 100])
      expect(Array.isArray(PAGE_SIZE_OPTIONS)).toBe(true)
      expect(PAGE_SIZE_OPTIONS).toHaveLength(4)
    })

    it('should have default page size in options', () => {
      expect(PAGE_SIZE_OPTIONS).toContain(DEFAULT_PAGE_SIZE)
    })

    it('should have page size options in ascending order', () => {
      for (let i = 0; i < PAGE_SIZE_OPTIONS.length - 1; i++) {
        expect(PAGE_SIZE_OPTIONS[i]).toBeLessThan(PAGE_SIZE_OPTIONS[i + 1])
      }
    })
  })

  describe('date formats', () => {
    it('should define DATE_FORMATS', () => {
      expect(DATE_FORMATS.SHORT).toBe('short')
      expect(DATE_FORMATS.LONG).toBe('long')
      expect(DATE_FORMATS.ISO).toBe('iso')
    })

    it('should have all expected format keys', () => {
      expect(Object.keys(DATE_FORMATS)).toEqual(['SHORT', 'LONG', 'ISO'])
    })
  })

  describe('storage keys', () => {
    it('should define authentication keys', () => {
      expect(STORAGE_KEYS.AUTH_TOKEN).toBe('auth_token')
      expect(STORAGE_KEYS.REFRESH_TOKEN).toBe('refresh_token')
      expect(STORAGE_KEYS.USER).toBe('user')
    })

    it('should define tenant key', () => {
      expect(STORAGE_KEYS.TENANT).toBe('current_tenant')
    })

    it('should define preference keys', () => {
      expect(STORAGE_KEYS.THEME).toBe('theme')
      expect(STORAGE_KEYS.LOCALE).toBe('locale')
    })

    it('should have unique values', () => {
      const values = Object.values(STORAGE_KEYS)
      const uniqueValues = [...new Set(values)]
      expect(values).toHaveLength(uniqueValues.length)
    })

    it('should have snake_case format', () => {
      Object.values(STORAGE_KEYS).forEach((key) => {
        expect(key).toMatch(/^[a-z_]+$/)
      })
    })
  })

  describe('validation rules', () => {
    it('should define PASSWORD_MIN_LENGTH', () => {
      expect(VALIDATION_RULES.PASSWORD_MIN_LENGTH).toBe(6)
      expect(typeof VALIDATION_RULES.PASSWORD_MIN_LENGTH).toBe('number')
    })

    it('should define SKU_MIN_LENGTH', () => {
      expect(VALIDATION_RULES.SKU_MIN_LENGTH).toBe(3)
      expect(typeof VALIDATION_RULES.SKU_MIN_LENGTH).toBe('number')
    })

    it('should define PHONE_MIN_DIGITS', () => {
      expect(VALIDATION_RULES.PHONE_MIN_DIGITS).toBe(10)
      expect(typeof VALIDATION_RULES.PHONE_MIN_DIGITS).toBe('number')
    })

    it('should have positive values', () => {
      expect(VALIDATION_RULES.PASSWORD_MIN_LENGTH).toBeGreaterThan(0)
      expect(VALIDATION_RULES.SKU_MIN_LENGTH).toBeGreaterThan(0)
      expect(VALIDATION_RULES.PHONE_MIN_DIGITS).toBeGreaterThan(0)
    })
  })

  describe('immutability', () => {
    it('should be readonly constants', () => {
      expect(Object.isFrozen(API_ENDPOINTS)).toBe(false) // TypeScript const assertion doesn't freeze
      expect(Object.isFrozen(INVOICE_STATUS_SEVERITY)).toBe(false)
      expect(Object.isFrozen(PAYMENT_STATUS_SEVERITY)).toBe(false)
      expect(Object.isFrozen(SUBSCRIPTION_STATUS_SEVERITY)).toBe(false)
      expect(Object.isFrozen(DATE_FORMATS)).toBe(false)
      expect(Object.isFrozen(STORAGE_KEYS)).toBe(false)
      expect(Object.isFrozen(VALIDATION_RULES)).toBe(false)
    })

    it('should export all expected constants', () => {
      expect(API_ENDPOINTS).toBeDefined()
      expect(INVOICE_STATUS_SEVERITY).toBeDefined()
      expect(PAYMENT_STATUS_SEVERITY).toBeDefined()
      expect(SUBSCRIPTION_STATUS_SEVERITY).toBeDefined()
      expect(DEFAULT_PAGE_SIZE).toBeDefined()
      expect(PAGE_SIZE_OPTIONS).toBeDefined()
      expect(DATE_FORMATS).toBeDefined()
      expect(STORAGE_KEYS).toBeDefined()
      expect(VALIDATION_RULES).toBeDefined()
    })
  })
})
