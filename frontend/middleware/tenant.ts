export default defineNuxtRouteMiddleware(async () => {
  // Skip during SSR to prevent hydration issues with localStorage
  if (import.meta.server) {
    return
  }

  const tenantStore = useTenantStore()
  const authStore = useAuthStore()

  // Skip if not authenticated
  if (!authStore.isAuthenticated) {
    return
  }

  // Only fetch if we don't have tenants and no current tenant is set
  // This prevents unnecessary API calls on page reload when state is restored
  if (tenantStore.availableTenants.length === 0 && !tenantStore.currentTenantId) {
    try {
      await tenantStore.fetchAvailableTenants()
    }
    catch (error) {
      console.error('Failed to fetch tenants:', error)
    }
  }

  // If we have a currentTenantId but no permissions (token without role), call selectTenant to get permissions
  if (tenantStore.currentTenantId && authStore.permissions.length === 0) {
    try {
      await authStore.selectTenant(tenantStore.currentTenantId)
    }
    catch (error) {
      console.error('Failed to select tenant:', error)
    }
  }

  // If we have a currentTenantId but no currentTenant object, restore it
  if (tenantStore.currentTenantId && !tenantStore.currentTenant && tenantStore.availableTenants.length > 0) {
    tenantStore.selectTenant(tenantStore.currentTenantId)
  }

  // Require tenant selection for protected routes
  if (!tenantStore.hasTenant && tenantStore.availableTenants.length > 0) {
    // Auto-select first tenant (use authStore to get permissions)
    await authStore.selectTenant(tenantStore.availableTenants[0].id)
  }
})
