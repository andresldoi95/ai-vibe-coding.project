/**
 * Constants for the application
 */

// API endpoints
export const API_ENDPOINTS = {
  AUTH: {
    LOGIN: '/auth/login',
    LOGOUT: '/auth/logout',
    REFRESH: '/auth/refresh',
    ME: '/auth/me',
  },
  TENANTS: {
    LIST: '/tenants',
    DETAIL: (id: string) => `/tenants/${id}`,
  },
  BILLING: {
    INVOICES: '/billing/invoices',
    INVOICE_DETAIL: (id: string) => `/billing/invoices/${id}`,
    CUSTOMERS: '/billing/customers',
    CUSTOMER_DETAIL: (id: string) => `/billing/customers/${id}`,
    PAYMENTS: '/billing/payments',
  },
  INVENTORY: {
    PRODUCTS: '/inventory/products',
    PRODUCT_DETAIL: (id: string) => `/inventory/products/${id}`,
    WAREHOUSES: '/inventory/warehouses',
    STOCK_MOVEMENTS: '/inventory/stock-movements',
  },
} as const

// Status mappings
export const INVOICE_STATUS_SEVERITY = {
  draft: 'secondary',
  sent: 'info',
  paid: 'success',
  overdue: 'danger',
  cancelled: 'warning',
} as const

export const PAYMENT_STATUS_SEVERITY = {
  pending: 'warning',
  completed: 'success',
  failed: 'danger',
  refunded: 'info',
} as const

export const SUBSCRIPTION_STATUS_SEVERITY = {
  active: 'success',
  cancelled: 'warning',
  expired: 'danger',
  trial: 'info',
} as const

// Pagination defaults
export const DEFAULT_PAGE_SIZE = 10
export const PAGE_SIZE_OPTIONS = [10, 25, 50, 100]

// Date formats
export const DATE_FORMATS = {
  SHORT: 'short',
  LONG: 'long',
  ISO: 'iso',
} as const

// Local storage keys
export const STORAGE_KEYS = {
  AUTH_TOKEN: 'auth_token',
  REFRESH_TOKEN: 'refresh_token',
  USER: 'user',
  TENANT: 'current_tenant',
  THEME: 'theme',
  LOCALE: 'locale',
} as const

// Validation rules
export const VALIDATION_RULES = {
  PASSWORD_MIN_LENGTH: 6,
  SKU_MIN_LENGTH: 3,
  PHONE_MIN_DIGITS: 10,
} as const
