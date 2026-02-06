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
      const toast = useNotification()

      try {
        const tenant = await apiFetch<Tenant>(`/tenants/${tenantId}`)
        currentTenantId.value = tenantId
        currentTenant.value = tenant

        toast.showInfo('Tenant switched', `Now working on: ${tenant.name}`)
      }
      catch (error) {
        const errMessage
          = error instanceof Error ? error.message : 'Failed to switch tenant'
        toast.showError('Failed to switch tenant', errMessage)
        throw error
      }
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

    return {
      currentTenantId,
      currentTenant,
      availableTenants,
      hasTenant,
      setTenant,
      fetchAvailableTenants,
      clearTenant,
    }
  },
  {
    persist: {
      storage: persistedState.localStorage,
      paths: ['currentTenantId', 'currentTenant'],
    },
  },
)
