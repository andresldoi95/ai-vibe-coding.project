import { defineStore } from 'pinia'
import type { ApiResponse, LoginCredentials, LoginResponse, User } from '~/types/auth'

export const useAuthStore = defineStore(
  'auth',
  () => {
    const token = ref<string | null>(null)
    const user = ref<User | null>(null)
    const refreshToken = ref<string | null>(null)

    const isAuthenticated = computed(() => !!token.value && !!user.value)

    const login = async (credentials: LoginCredentials): Promise<LoginResponse> => {
      const { apiFetch } = useApi()

      // eslint-disable-next-line no-console
      console.log('[AuthStore] Logging in...', credentials.email)

      // Backend wraps response in { data, message, success }
      const apiResponse = await apiFetch<ApiResponse<LoginResponse>>('/auth/login', {
        method: 'POST',
        body: credentials,
      })

      // eslint-disable-next-line no-console
      console.log('[AuthStore] API Response:', apiResponse)

      const response = apiResponse.data

      token.value = response.accessToken
      refreshToken.value = response.refreshToken
      user.value = response.user

      // Set available tenants and select the first one
      const tenantStore = useTenantStore()
      if (response.tenants && response.tenants.length > 0) {
        // eslint-disable-next-line no-console
        console.log('[AuthStore] Setting tenants:', response.tenants)
        tenantStore.setAvailableTenants(response.tenants)
        // Auto-select first tenant
        tenantStore.selectTenant(response.tenants[0].id)
      }

      // eslint-disable-next-line no-console
      console.log('[AuthStore] Login complete')
      return response
    }

    const logout = () => {
      token.value = null
      user.value = null
      refreshToken.value = null

      // Clear tenant store on logout
      const tenantStore = useTenantStore()
      tenantStore.clearTenant()
    }

    const refreshAccessToken = async (): Promise<void> => {
      if (!refreshToken.value) {
        throw new Error('No refresh token available')
      }

      const { apiFetch } = useApi()

      try {
        const response = await apiFetch<{ accessToken: string }>(
          '/auth/refresh',
          {
            method: 'POST',
            body: { refreshToken: refreshToken.value },
          },
        )

        token.value = response.accessToken
      }
      catch (error) {
        logout()
        throw error
      }
    }

    const fetchCurrentUser = async (): Promise<void> => {
      const { apiFetch } = useApi()

      try {
        const apiResponse = await apiFetch<ApiResponse<User>>('/auth/me')
        user.value = apiResponse.data
      }
      catch (error) {
        logout()
        throw error
      }
    }

    return {
      token,
      user,
      refreshToken,
      isAuthenticated,
      login,
      logout,
      refreshAccessToken,
      fetchCurrentUser,
    }
  },
  {
    persist: {
      storage: persistedState.localStorage,
      paths: ['token', 'refreshToken', 'user'],
    },
  },
)
