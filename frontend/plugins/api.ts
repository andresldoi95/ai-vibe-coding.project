export default defineNuxtPlugin(() => {
  const config = useRuntimeConfig()

  const apiFetch = $fetch.create({
    baseURL: config.public.apiBase as string,
    async onRequest({ options }) {
      const authStore = useAuthStore()
      const tenantStore = useTenantStore()

      // Add authorization token
      if (authStore.token) {
        options.headers = {
          ...options.headers,
          Authorization: `Bearer ${authStore.token}`,
        }
      }

      // Add tenant context
      if (tenantStore.currentTenantId) {
        options.headers = {
          ...options.headers,
          'X-Tenant-Id': tenantStore.currentTenantId,
        }
      }
    },
    async onResponseError({ response }) {
      const authStore = useAuthStore()

      // Handle authentication errors
      if (response.status === 401) {
        authStore.logout()
        await navigateTo('/login')
      }
    },
  })

  return {
    provide: {
      apiFetch,
    },
  }
})
