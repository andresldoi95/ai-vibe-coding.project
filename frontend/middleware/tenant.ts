export default defineNuxtRouteMiddleware(async () => {
  const tenantStore = useTenantStore()
  const authStore = useAuthStore()

  // Skip if not authenticated
  if (!authStore.isAuthenticated) {
    return
  }

  // Fetch available tenants if not loaded
  if (tenantStore.availableTenants.length === 0) {
    try {
      await tenantStore.fetchAvailableTenants()
    }
    catch (error) {
      console.error('Failed to fetch tenants:', error)
    }
  }

  // Require tenant selection for protected routes
  if (!tenantStore.hasTenant && tenantStore.availableTenants.length > 0) {
    // Auto-select first tenant
    await tenantStore.setTenant(tenantStore.availableTenants[0].id)
  }
})
