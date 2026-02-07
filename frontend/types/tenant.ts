// Tenant types
export interface Tenant {
  id: string
  name: string
  slug: string
  status: string
}

export interface TenantSettings {
  tenantId: string
  currency: string
  timezone: string
  dateFormat: string
  locale: string
}
