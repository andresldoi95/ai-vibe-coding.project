export default defineNuxtPlugin(() => {
  const config = useRuntimeConfig()

  // Use server-side URL for SSR, client-side URL for browser
  const baseURL = import.meta.server
    ? config.apiBaseUrl
    : config.public.apiBase

  const apiFetch = $fetch.create({
    baseURL: baseURL as string,
    async onRequest({ options }) {
      const authStore = useAuthStore()
      const tenantStore = useTenantStore()

      // Initialize headers if not present
      options.headers = options.headers || {}

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

      // Log request for debugging
      if (import.meta.dev) {
        // eslint-disable-next-line no-console
        console.log('[API Request]', options.method || 'GET', options.baseURL, {
          hasAuth: !!authStore.token,
          hasTenant: !!tenantStore.currentTenantId,
        })
      }
    },
    async onResponse({ response }) {
      // Log successful responses for debugging
      if (import.meta.dev) {
        // eslint-disable-next-line no-console
        console.log('[API Response]', response.status, response._data)
      }
    },
    async onResponseError({ response }) {
      const authStore = useAuthStore()

      // Log errors for debugging
      if (import.meta.dev) {
        console.error('[API Error]', response.status, response._data)
      }

      // Handle authentication errors
      if (response.status === 401) {
        authStore.logout()
        await navigateTo('/login')
      }

      // Handle authorization/permission errors
      if (response.status === 403) {
        const { showPermissionError } = useNotification()
        showPermissionError()
      }

      // Extract error message from API response and throw it
      const errorData = response._data
      let errorMessage = 'An error occurred'

      if (errorData) {
        if (typeof errorData === 'string') {
          errorMessage = errorData
        }
        else if (errorData.message) {
          errorMessage = errorData.message
        }
        else if (errorData.error) {
          errorMessage = errorData.error
        }
        else if (errorData.errors && Array.isArray(errorData.errors) && errorData.errors.length > 0) {
          errorMessage = errorData.errors.join(', ')
        }
        else if (errorData.errors && typeof errorData.errors === 'object') {
          // Handle validation errors object { fieldName: ["error1", "error2"] }
          const validationErrors = Object.values(errorData.errors).flat()
          if (validationErrors.length > 0) {
            errorMessage = validationErrors.join(', ')
          }
        }
      }

      // Throw error with extracted message so it can be caught by try/catch
      throw new Error(errorMessage)
    },
  })

  return {
    provide: {
      apiFetch,
    },
  }
})
