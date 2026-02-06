// Tenant types
export interface Tenant {
  id: string
  name: string
  slug: string
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface TenantSettings {
  tenantId: string
  currency: string
  timezone: string
  dateFormat: string
  locale: string
}
