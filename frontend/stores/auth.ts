import { defineStore } from 'pinia'
import type { LoginCredentials, LoginResponse, User } from '~/types/auth'

export const useAuthStore = defineStore(
  'auth',
  () => {
    const token = ref<string | null>(null)
    const user = ref<User | null>(null)
    const refreshToken = ref<string | null>(null)

    const isAuthenticated = computed(() => !!token.value && !!user.value)

    const login = async (credentials: LoginCredentials): Promise<void> => {
      const { apiFetch } = useApi()
      const toast = useNotification()

      try {
        const response = await apiFetch<LoginResponse>('/auth/login', {
          method: 'POST',
          body: credentials,
        })

        token.value = response.accessToken
        refreshToken.value = response.refreshToken
        user.value = response.user

        toast.showSuccess(
          'Login successful',
          `Welcome back, ${response.user.name}!`,
        )
      }
      catch (error) {
        const errMessage
          = error instanceof Error ? error.message : 'Invalid credentials'
        toast.showError('Login failed', errMessage)
        throw error
      }
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
        user.value = await apiFetch<User>('/auth/me')
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
