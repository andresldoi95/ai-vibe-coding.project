/**
 * Hydration plugin to ensure Pinia stores are properly restored before API calls
 * This must run before the api plugin (hence the 01. prefix)
 */
export default defineNuxtPlugin(() => {
  const authStore = useAuthStore()
  const tenantStore = useTenantStore()

  // Force hydration by accessing the stores
  // This ensures persisted state is loaded from localStorage before any API calls
  if (import.meta.client) {
    // eslint-disable-next-line no-console
    console.log('[Hydration] Auth state:', {
      isAuthenticated: authStore.isAuthenticated,
      hasToken: !!authStore.token,
      hasUser: !!authStore.user,
    })

    // eslint-disable-next-line no-console
    console.log('[Hydration] Tenant state:', {
      currentTenantId: tenantStore.currentTenantId,
      hasTenant: tenantStore.hasTenant,
      availableTenants: tenantStore.availableTenants.length,
    })
  }
})
