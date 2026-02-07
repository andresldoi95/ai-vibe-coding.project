import { defineStore } from 'pinia'
import type { Tenant } from '~/types/tenant'

export const useTenantStore = defineStore(
  'tenant',
  () => {
    const currentTenantId = ref<string | null>(null)
    const currentTenant = ref<Tenant | null>(null)
    const availableTenants = ref<Tenant[]>([])

    const hasTenant = computed(() => !!currentTenantId.value)

    const setTenant = async (tenantId: string): Promise<void> => {
      const { apiFetch } = useApi()

      const tenant = await apiFetch<Tenant>(`/tenants/${tenantId}`)
      currentTenantId.value = tenantId
      currentTenant.value = tenant
    }

    const fetchAvailableTenants = async (): Promise<void> => {
      const { apiFetch } = useApi()

      try {
        availableTenants.value = await apiFetch<Tenant[]>('/tenants')

        // Auto-select first tenant if none selected
        if (!currentTenantId.value && availableTenants.value.length > 0) {
          await setTenant(availableTenants.value[0].id)
        }
      }
      catch (error) {
        console.error('Failed to fetch tenants:', error)
        throw error
      }
    }

    const clearTenant = () => {
      currentTenantId.value = null
      currentTenant.value = null
      availableTenants.value = []
    }

    const setAvailableTenants = (tenants: Tenant[]) => {
      availableTenants.value = tenants
    }

    const selectTenant = (tenantId: string) => {
      const tenant = availableTenants.value.find(t => t.id === tenantId)
      if (tenant) {
        currentTenantId.value = tenantId
        currentTenant.value = tenant
        // eslint-disable-next-line no-console
        console.log('[TenantStore] Tenant selected:', tenant.name)
      }
      else {
        console.warn('[TenantStore] Tenant not found in available tenants:', tenantId)
      }
    }

    return {
      currentTenantId,
      currentTenant,
      availableTenants,
      hasTenant,
      setTenant,
      fetchAvailableTenants,
      clearTenant,
      setAvailableTenants,
      selectTenant,
    }
  },
  {
    persist: {
      storage: persistedState.localStorage,
      paths: ['currentTenantId', 'currentTenant', 'availableTenants'],
    },
  },
)
